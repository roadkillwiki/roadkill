﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Text.ToC
{
	/// <summary>
	/// Keeps a tree of <see cref="Item"/> objects, from the Root H1 tags down.
	/// </summary>
	internal class Tree
	{
		public Item Root { get; set; }
		private Item _currentItem;
		private int _currentLevel;
		private StringTemplate _stringTemplate;
		private List<string> _titleIdLookup;
		private static Random _random = new Random();

		public Tree(StringTemplate stringTemplate)
		{
			Root = new Item("Not used", "");
			_currentItem = Root;
			_stringTemplate = stringTemplate;
			_titleIdLookup = new List<string>();
		}

		public Item AddItemAtLevel(int level, string title)
		{
			// Generate a unique id for the A tag
			string titleId = title.EncodeTitle();
			if (_titleIdLookup.Contains(titleId))
			{
				int uniqueSuffix = _titleIdLookup.Count(x => x.StartsWith(titleId)) + 1;
				titleId += "-"+uniqueSuffix;
			}
			_titleIdLookup.Add(titleId);

			Item item = new Item(title, titleId);
			if (level == _currentLevel)
			{
				if (_currentItem.Parent != null)
				{
					_currentItem.Parent.AddChild(item);
				}
				else
				{
					_currentItem.AddChild(item);
				}
			}
			else if (level > _currentLevel)
			{
				_currentItem.AddChild(item);
			}
			else if (level < _currentLevel)
			{
				// e.g. at level h4, and a h1 tag appears
				// h4.parent = h3
				// h3.parent = h2
				// h2.parent = h1
				// (ding, same level)
				Item parent = _currentItem.Parent;
				while (parent != null && parent.Parent != null && parent.Level > level)
				{
					parent = parent.Parent;
				}

				if (parent != null && parent.Parent != null)
					parent.Parent.AddChild(item);
			}

			_currentItem = item;
			_currentLevel = level;

			return item;
		}

		public string CreateHtmlForItems()
		{
			return _stringTemplate.CreateHtml(Root);
		}
	}
}
