using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Extensions.Tables;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Roadkill.Core.Converters
{
	/// <summary>
	/// A Markdown parser based on Markdig (CommonMark), with the GitHub Flavored Markdown extensions (pipe tables,
	/// strikethrough, task lists, extended autolinks, footnotes) and Mermaid diagrams (```mermaid code blocks).
	/// </summary>
	/// <remarks>
	/// This replaces the MarkdownSharp based parser used by Roadkill 2.x, and keeps its Roadkill specific behaviour:
	/// <list type="bullet">
	/// <item>The <see cref="LinkParsed"/> and <see cref="ImageParsed"/> events are raised for every link and image,
	/// so internal wiki links, attachments and Special: pages keep working.</item>
	/// <item>External links get rel="nofollow"; images get the "img-responsive" class.</item>
	/// <item>Image dimensions: ![alt](url =250x350), ![alt](url =250x) or ![alt](url "title" =x100).</item>
	/// <item>ATX headings without a space after the #'s (#Heading, ##Heading##) are still headings.</item>
	/// </list>
	/// </remarks>
	public class MarkdownParser : IMarkupParser
	{
		internal const string ImageWidthKey = "roadkill-image-width";
		internal const string ImageHeightKey = "roadkill-image-height";

		private static readonly MarkdownPipeline _pipeline = CreatePipeline();

		public MarkupParserHelp MarkupParserHelp { get; private set; }

		/// <summary>
		/// Occurs when an image tag is parsed.
		/// </summary>
		public event EventHandler<ImageEventArgs> ImageParsed;

		/// <summary>
		/// Occurs when a hyperlink is parsed.
		/// </summary>
		public event EventHandler<LinkEventArgs> LinkParsed;

		/// <summary>
		/// Creates a new Markdown parser.
		/// </summary>
		public MarkdownParser()
		{
			MarkupParserHelp = new MarkupParserHelp()
			{
				BoldToken = "**",
				ItalicToken = "*",
				UnderlineToken = "",
				LinkStartToken = "[%LINKTEXT%",
				LinkEndToken = "](%URL%)",
				ImageStartToken = "![%ALT%",
				ImageEndToken = "](%FILENAME%)",
				BulletListToken = "*",
				NumberedListToken = "1.",
				HeadingToken = "#",
			};
		}

		/// <summary>
		/// Creates the Markdig pipeline used by Roadkill. The pipeline is immutable and thread safe once built.
		/// </summary>
		public static MarkdownPipeline CreatePipeline()
		{
			MarkdownPipelineBuilder builder = new MarkdownPipelineBuilder()
				.UsePipeTables(new PipeTableOptions())
				.UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
				.UseTaskLists()
				.UseAutoLinks()
				.UseFootnotes()
				.UseDiagrams();

			// Roadkill's image size syntax, ![alt](url =WIDTHxHEIGHT), is tried before the standard link parser.
			builder.InlineParsers.InsertBefore<LinkInlineParser>(new SizedImageInlineParser());

			return builder.Build();
		}

		/// <summary>
		/// Transforms the provided Markdown-formatted text to HTML.
		/// </summary>
		public string Transform(string text)
		{
			if (string.IsNullOrEmpty(text))
				return "";

			text = RelaxedHeadings.AddMissingSpaces(text, _pipeline);
			MarkdownDocument document = Markdig.Markdown.Parse(text, _pipeline);

			using (StringWriter writer = new StringWriter())
			{
				HtmlRenderer renderer = new HtmlRenderer(writer);
				_pipeline.Setup(renderer);

				renderer.ObjectRenderers.Replace<LinkInlineRenderer>(new RoadkillLinkInlineRenderer(this));
				renderer.ObjectRenderers.Replace<AutolinkInlineRenderer>(new RoadkillAutolinkInlineRenderer(this));

				renderer.Render(document);
				writer.Flush();

				return writer.ToString();
			}
		}

		/// <summary>
		/// Raises the <see cref="ImageParsed"/> event.
		/// </summary>
		protected internal void OnImageParsed(ImageEventArgs e)
		{
			ImageParsed?.Invoke(this, e);
		}

		/// <summary>
		/// Raises the <see cref="LinkParsed"/> event.
		/// </summary>
		protected internal void OnLinkParsed(LinkEventArgs e)
		{
			LinkParsed?.Invoke(this, e);
		}

		/// <summary>
		/// Gets the text of an inline and its children, without any markup (used for link text and image alt text).
		/// </summary>
		internal static string GetPlainText(Inline inline)
		{
			StringBuilder builder = new StringBuilder();
			AppendPlainText(inline, builder);
			return builder.ToString();
		}

		private static void AppendPlainText(Inline inline, StringBuilder builder)
		{
			switch (inline)
			{
				case LiteralInline literal:
					builder.Append(literal.Content.ToString());
					break;

				case CodeInline code:
					builder.Append(code.Content);
					break;

				case LineBreakInline _:
					builder.Append(' ');
					break;

				case AutolinkInline autolink:
					builder.Append(autolink.Url);
					break;

				case ContainerInline container:
					foreach (Inline child in container)
					{
						AppendPlainText(child, builder);
					}
					break;
			}
		}

		/// <summary>
		/// Renders links and images, raising the <see cref="LinkParsed"/> and <see cref="ImageParsed"/> events so the
		/// hrefs and srcs can be translated (internal links, attachments), in the same HTML format as Roadkill 2.x.
		/// </summary>
		private class RoadkillLinkInlineRenderer : HtmlObjectRenderer<LinkInline>
		{
			private readonly MarkdownParser _parser;

			public RoadkillLinkInlineRenderer(MarkdownParser parser)
			{
				_parser = parser;
			}

			protected override void Write(HtmlRenderer renderer, LinkInline link)
			{
				string url = link.GetDynamicUrl != null ? (link.GetDynamicUrl() ?? link.Url) : link.Url;
				url = url ?? "";

				if (link.IsImage)
					WriteImage(renderer, link, url);
				else
					WriteLink(renderer, link, url);
			}

			private void WriteImage(HtmlRenderer renderer, LinkInline link, string url)
			{
				string alt = GetPlainText(link);
				ImageEventArgs args = new ImageEventArgs(url, url, alt, link.Title ?? "");
				_parser.OnImageParsed(args);

				renderer.Write("<img src=\"").WriteEscape(args.Src).Write("\"");
				renderer.Write(" class=\"img-responsive\" border=\"0\"");
				renderer.Write(" alt=\"").WriteEscape(args.Alt).Write("\"");

				string width = link.GetData(ImageWidthKey) as string;
				string height = link.GetData(ImageHeightKey) as string;

				if (!string.IsNullOrEmpty(width))
					renderer.Write(" width=\"").Write(width).Write("px\"");

				if (!string.IsNullOrEmpty(height))
					renderer.Write(" height=\"").Write(height).Write("px\"");

				if (!string.IsNullOrEmpty(args.Title))
					renderer.Write(" title=\"").WriteEscape(args.Title).Write("\"");

				renderer.Write(" />");
			}

			private void WriteLink(HtmlRenderer renderer, LinkInline link, string url)
			{
				string text = GetPlainText(link);
				LinkEventArgs args = new LinkEventArgs(url, url, text, "");
				_parser.OnLinkParsed(args);

				renderer.Write("<a href=\"").WriteEscape(args.Href).Write("\"");

				if (!args.IsInternalLink)
					renderer.Write(" rel=\"nofollow\"");

				if (!string.IsNullOrEmpty(link.Title))
					renderer.Write(" title=\"").WriteEscape(link.Title).Write("\"");

				if (!string.IsNullOrEmpty(args.Target))
					renderer.Write(" target=\"").WriteEscape(args.Target).Write("\"");

				if (!string.IsNullOrEmpty(args.CssClass))
					renderer.Write(" class=\"").WriteEscape(args.CssClass).Write("\"");

				renderer.Write(">");

				// Only use the event's text if a handler changed it, otherwise keep any formatting in the link text.
				if (args.Text != text)
					renderer.WriteEscape(args.Text);
				else
					renderer.WriteChildren(link);

				renderer.Write("</a>");
			}
		}

		/// <summary>
		/// Renders &lt;http://...&gt; style autolinks, raising the <see cref="LinkParsed"/> event.
		/// </summary>
		private class RoadkillAutolinkInlineRenderer : HtmlObjectRenderer<AutolinkInline>
		{
			private readonly MarkdownParser _parser;

			public RoadkillAutolinkInlineRenderer(MarkdownParser parser)
			{
				_parser = parser;
			}

			protected override void Write(HtmlRenderer renderer, AutolinkInline link)
			{
				string url = link.IsEmail ? "mailto:" + link.Url : link.Url;
				LinkEventArgs args = new LinkEventArgs(url, url, link.Url, "");
				_parser.OnLinkParsed(args);

				renderer.Write("<a href=\"").WriteEscape(args.Href).Write("\"");

				if (!args.IsInternalLink && !link.IsEmail)
					renderer.Write(" rel=\"nofollow\"");

				if (!string.IsNullOrEmpty(args.Target))
					renderer.Write(" target=\"").WriteEscape(args.Target).Write("\"");

				if (!string.IsNullOrEmpty(args.CssClass))
					renderer.Write(" class=\"").WriteEscape(args.CssClass).Write("\"");

				renderer.Write(">").WriteEscape(args.Text).Write("</a>");
			}
		}

		/// <summary>
		/// Parses Roadkill's image size syntax: ![alt](url =250x350), ![alt](url =250x), ![alt](url "title" =x350).
		/// This isn't valid CommonMark, so without this parser those images would be output as text.
		/// </summary>
		private class SizedImageInlineParser : InlineParser
		{
			private static readonly Regex _sizedImageRegex = new Regex(
				@"\G!\[(?<alt>[^\]]*)\]\((?<ws>[ ]*)(?:<(?<url>[^>\n]*)>|(?<url>[^\s()]+))(?:\s+(?<q>[""'])(?<title>.*?)\k<q>)?\s+=(?<width>\d*)x(?<height>\d*)[ ]*\)",
				RegexOptions.Compiled);

			public SizedImageInlineParser()
			{
				OpeningCharacters = new[] { '!' };
			}

			public override bool Match(InlineProcessor processor, ref StringSlice slice)
			{
				if (slice.PeekChar() != '[')
					return false;

				Match match = _sizedImageRegex.Match(slice.Text, slice.Start);
				if (!match.Success || match.Index + match.Length - 1 > slice.End)
					return false;

				string width = match.Groups["width"].Value;
				string height = match.Groups["height"].Value;
				if (width.Length == 0 && height.Length == 0)
					return false;

				int start = processor.GetSourcePosition(slice.Start, out int line, out int column);

				LinkInline image = new LinkInline(match.Groups["url"].Value, match.Groups["title"].Value)
				{
					IsImage = true,
					IsClosed = true,
					Line = line,
					Column = column,
					Span = new SourceSpan(start, start + match.Length - 1)
				};

				image.AppendChild(new LiteralInline(match.Groups["alt"].Value));
				image.SetData(ImageWidthKey, width);
				image.SetData(ImageHeightKey, height);

				processor.Inline = image;
				slice.Start += match.Length;

				return true;
			}
		}

		/// <summary>
		/// Roadkill 2.x (MarkdownSharp) treated "#Heading" (no space after the #'s) as a heading and removed the
		/// closing #'s of "#Heading#". CommonMark requires the spaces, so they are added to the source before parsing.
		/// Only paragraph lines are changed, so code blocks, HTML blocks, tables and standard CommonMark headings
		/// ("# Using C#") are left alone.
		/// </summary>
		private static class RelaxedHeadings
		{
			private static readonly Regex _tableSeparatorRegex = new Regex(@"^\s*\|?\s*:?-+:?\s*(\|\s*:?-+:?\s*)*\|?\s*$", RegexOptions.Compiled);

			[ThreadStatic]
			private static List<ClosedBlockLines> _closedBlocks;

			/// <summary>
			/// The source lines of a closed paragraph block.
			/// </summary>
			private class ClosedBlockLines
			{
				public List<StringSlice> Lines { get; } = new List<StringSlice>();
			}

			// A separate pipeline, only used to find the paragraph lines. Their source lines are captured
			// when each block is closed, as Markdig releases them once the inlines have been parsed.
			private static readonly MarkdownPipeline _prePassPipeline = CreatePrePassPipeline();

			private static MarkdownPipeline CreatePrePassPipeline()
			{
				MarkdownPipelineBuilder builder = new MarkdownPipelineBuilder();
				builder.BlockParsers.Find<ParagraphBlockParser>().Closed += OnBlockClosed;
								builder.InlineParsers.Clear();

				return builder.Build();
			}

			private static void OnBlockClosed(BlockProcessor processor, Block block)
			{
				if (_closedBlocks == null || !(block is LeafBlock leaf) || leaf.Lines.Lines == null)
					return;

				// Copy the line positions now, as the lines are released after the inlines are processed.
				var closed = new ClosedBlockLines();

				for (int i = 0; i < leaf.Lines.Count; i++)
				{
					closed.Lines.Add(leaf.Lines.Lines[i].Slice);
				}

				_closedBlocks.Add(closed);
			}

			public static string AddMissingSpaces(string text, MarkdownPipeline pipeline)
			{
				if (text.IndexOf('#') < 0)
					return text;

				SortedSet<int> insertPositions = new SortedSet<int>();
				_closedBlocks = new List<ClosedBlockLines>();

				try
				{
					Markdig.Markdown.Parse(text, _prePassPipeline);
					foreach (ClosedBlockLines block in _closedBlocks)
					{
						AddPositionsForBlock(block, text, insertPositions);
					}
				}
				finally
				{
					_closedBlocks = null;
				}

				if (insertPositions.Count == 0)
					return text;

				StringBuilder builder = new StringBuilder(text);
				foreach (int position in insertPositions.Reverse())
				{
					builder.Insert(position, ' ');
				}

				return builder.ToString();
			}

			private static void AddPositionsForBlock(ClosedBlockLines block, string text, SortedSet<int> positions)
			{
				// Paragraphs that are pipe tables are left alone.
				if (block.Lines.Any(line => _tableSeparatorRegex.IsMatch(line.ToString())))
					return;

				foreach (StringSlice line in block.Lines)
				{
					AddPositionsForLine(line, text, positions);
				}
			}

			private static void AddPositionsForLine(StringSlice slice, string text, SortedSet<int> positions)
			{
				// The slices are positions in the original text: ignore any that aren't.
				if (!ReferenceEquals(slice.Text, text) || slice.Start < 0 || slice.End >= text.Length || slice.Start > slice.End)
					return;

				int start = slice.Start;
				int end = slice.End;

				// "#Heading" : 1-6 #'s followed by a character that isn't a space or #.
				while (start <= end && text[start] == ' ' && start - slice.Start < 3)
					start++;

				int hashCount = 0;
				while (start + hashCount <= end && text[start + hashCount] == '#')
					hashCount++;

				int afterHashes = start + hashCount;
				if (hashCount < 1 || hashCount > 6 || afterHashes > end || char.IsWhiteSpace(text[afterHashes]))
					return;

				positions.Add(afterHashes);
				start = afterHashes;

				// Closing #'s without a space before them: "#Heading#" becomes "# Heading #".
				while (end >= start && char.IsWhiteSpace(text[end]))
					end--;

				int closingStart = end;
				while (closingStart >= start && text[closingStart] == '#')
					closingStart--;

				bool hasClosingHashes = closingStart < end;
				bool hasContent = closingStart >= start;
				if (hasClosingHashes && hasContent && text[closingStart] != ' ' && text[closingStart] != '\\')
					positions.Add(closingStart + 1);
			}
		}
	}
}
