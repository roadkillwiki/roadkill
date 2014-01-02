using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Attachments
{
	/// <summary>
	/// Helper methods for the attachments folder.
	/// </summary>
	public class AttachmentPathUtil
	{
		private ApplicationSettings _settings;

		public AttachmentPathUtil(ApplicationSettings settings)
		{
			_settings = settings;
		}

		/// <summary>
		/// Returns the physical path of the Attachments folder plus the parsed relative file path.
		/// </summary>
		/// <param name="relativePath">A url style path, e.g. /folder1/folder2/</param>
		/// <returns>Returns the physical path of the Attachments folder plus the parsed 
		/// relative path parameter joined. This path always contains a trailing slash (or / on Unix based systems).</returns>
		public string ConvertUrlPathToPhysicalPath(string relativePath)
		{
			string dirSeperator = Path.DirectorySeparatorChar.ToString();
			string attachmentsPath = _settings.AttachmentsDirectoryPath;

			if (string.IsNullOrEmpty(relativePath))
				return attachmentsPath;

			// Strip out the /Attachments/ part of the path
			relativePath = relativePath.Replace(_settings.AttachmentsUrlPath, "");
			relativePath = relativePath.Replace("/", dirSeperator);

			// Remove all '\' ('/' on unix) from the start of the path.
			relativePath = relativePath.TrimStart(Path.DirectorySeparatorChar);

			if (!string.IsNullOrEmpty(relativePath))
				relativePath = Path.Combine(attachmentsPath, relativePath);
			else
				relativePath = attachmentsPath;

			if (!relativePath.EndsWith(dirSeperator))
				relativePath += dirSeperator;

			return relativePath;
		}

		/// <summary>
		/// Validates that the passed physical path is a valid sub folder of the base attachments path.
		/// </summary>
		/// <param name="physicalDirectoryPath">An absolute path which is a subdirectory of the route 
		/// attachments directory, e.g. c:\temp\attachments\folder1\folder2\</param>
		/// <returns>True if it's a valid subdirectory, false otherwise. If the path contains invalid path 
		/// characters (except for a directory seperator), a . or .. then false is returned.
		/// If <c>absoluteDirectoryPath</c> is an empty string, true is returned.</returns>
		public bool IsAttachmentPathValid(string physicalDirectoryPath, bool checkPathExists = true)
		{
			if (string.IsNullOrEmpty(physicalDirectoryPath))
				return true;

			physicalDirectoryPath = physicalDirectoryPath.Replace("/", Path.DirectorySeparatorChar.ToString());

			// .\, ..\, \., \\. are bad and should return false
			List<string> slashDots = new List<string>()
			{
				"."  +Path.DirectorySeparatorChar,
				".." +Path.DirectorySeparatorChar,
				Path.DirectorySeparatorChar + ".",
				Path.DirectorySeparatorChar + "..",
			};

			if (slashDots.Any(x => physicalDirectoryPath.Contains(x)))
				return false;

			// Ignore '\' (or '/' on unix) from the invalid char list as we have a full path
			List<char> invalidChars = Path.GetInvalidPathChars().ToList();
			invalidChars.Remove(Path.DirectorySeparatorChar);

			if (physicalDirectoryPath.IndexOfAny(invalidChars.ToArray()) > -1)
				return false;

			// Checks have passed
			if (!checkPathExists)
				return true;

			if (Directory.Exists(physicalDirectoryPath))
			{
				try
				{
					// Check the path passed isn't simply the attachments path with extra slashes etc.
					DirectoryInfo attachmentsDir = new DirectoryInfo(_settings.AttachmentsDirectoryPath);
					DirectoryInfo searchDir = new DirectoryInfo(physicalDirectoryPath);

					string attachmentsFullPath = attachmentsDir.FullName.TrimEnd('\\');
					string physicalFullPath = searchDir.FullName.TrimEnd('\\');

					if (attachmentsFullPath == physicalFullPath)
						return true;

					string directoryName = searchDir.Name;

					// Search for the subdirectory (it should exist) *under* the attachments directory.
					// This is safer (but slightly less performant) than doing a startswith
					DirectoryInfo[] searchSubDirs = attachmentsDir.GetDirectories(directoryName, SearchOption.AllDirectories);
					if (searchSubDirs.Length > 0)
						return true;
				}
				catch (ArgumentException)
				{
					// bad paths, e.g. if it's just a "\"
					return false;
				}
				catch (IOException)
				{
					return false;
				}
			}
			else
			{
				Log.Warn("The path {0} does not exist", physicalDirectoryPath);
			}

			return false;
		}

		/// <summary>
		/// Tests if the attachments folder provided can be written to, by writing a file to the folder.
		/// </summary>
		/// <param name="folder">The folder path which should include "~/" at the start.</param>
		/// <param name="context">The HttpContext to map a virtual (~) path too.</param>
		/// <returns>Any error messages or an empty string if no errors occurred.</returns>
		public static string AttachmentFolderExistsAndWriteable(string folder, HttpContextBase context)
		{
			string errors = "";
			if (string.IsNullOrEmpty(folder))
			{
				errors = "The folder name is empty";
			}
			else
			{
				try
				{
					string directory = folder;
					if (folder.StartsWith("~") && context != null)
						directory = context.Server.MapPath(folder);

					if (Directory.Exists(directory))
					{
						string path = Path.Combine(directory, "_installtest.txt");
						File.WriteAllText(path, "created by the installer to test the attachments folder");
						File.Delete(path);
					}
					else
					{
						// todo-translation
						errors = "The directory does not exist, please create it first";
					}
				}
				catch (Exception e)
				{
					errors = e.ToString();
				}
			}

			return errors;
		}
	}
}
