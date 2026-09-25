using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Exceptions;
using Roadkill.Core.Localization;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Roadkill.Core.Services
{
	/// <summary>
	/// Stores attachments in Azure blob storage (Azure.Storage.Blobs SDK).
	/// </summary>
	public class AzureFileService : IFileService
	{
		private readonly ApplicationSettings _applicationSettings;
		private readonly SettingsService _settingsService;
		private static readonly string[] FilesToExclude = new string[] { "emptyfile.txt", "_installtest.txt" }; // installer/publish files
		private static readonly Regex _multipleSlashRegex = new Regex(@"(\/+|\\+)", RegexOptions.Compiled);

		public AzureFileService(ApplicationSettings applicationSettings, SettingsService settingsService)
		{
			_applicationSettings = applicationSettings;
			_settingsService = settingsService;
		}

		public void Delete(string filePath, string fileName)
		{
			try
			{
				BlobContainerClient container = GetBlobContainer();
				string path = CleanPath(String.Format("/{0}/{1}", filePath, fileName));
				container.GetBlobClient(path).DeleteIfExists();
			}
			catch (RequestFailedException e)
			{
				throw new FileException(e, "Unable to delete {0} from {1}", fileName, filePath);
			}
		}

		public void DeleteFolder(string folderPath)
		{
			try
			{
				BlobContainerClient container = GetBlobContainer();
				string prefix = GetDirectoryPrefix(folderPath);

				List<BlobHierarchyItem> items = container.GetBlobsByHierarchy(BlobTraits.None, BlobStates.None, "/", prefix).ToList();
				bool hasFiles = items.Any(x => x.IsBlob && !FilesToExclude.Contains(Path.GetFileName(x.Blob.Name)));
				bool hasDirectories = items.Any(x => x.IsPrefix);

				if (hasFiles || hasDirectories)
					throw new FileException("The folder is not empty.", null);

				foreach (BlobHierarchyItem item in items.Where(x => x.IsBlob))
				{
					container.GetBlobClient(item.Blob.Name).DeleteIfExists();
				}
			}
			catch (RequestFailedException e)
			{
				throw new FileException(e.Message, e);
			}
		}

		public bool CreateFolder(string parentPath, string folderName)
		{
			try
			{
				BlobContainerClient container = GetBlobContainer();
				string fileName = CleanPath(parentPath + "/" + folderName + "/" + FilesToExclude[0]);
				BlobClient blob = container.GetBlobClient(fileName);

				if (!blob.Exists())
				{
					blob.Upload(BinaryData.FromString(String.Empty));
					return true;
				}

				throw new FileException(SiteStrings.FileManager_Error_CreateFolder + " " + folderName, null);
			}
			catch (RequestFailedException e)
			{
				throw new FileException(e.Message, e);
			}
		}

		public DirectoryViewModel FolderInfo(string dir)
		{
			try
			{
				string currentFolderName = dir;
				if (!string.IsNullOrEmpty(currentFolderName) && currentFolderName != "/")
					currentFolderName = Path.GetFileName(dir);

				DirectoryViewModel directoryModel = new DirectoryViewModel(currentFolderName, dir);
				BlobContainerClient container = GetBlobContainer();
				string prefix = GetDirectoryPrefix(dir);

				foreach (BlobHierarchyItem item in container.GetBlobsByHierarchy(BlobTraits.Metadata, BlobStates.None, "/", prefix))
				{
					if (item.IsPrefix)
					{
						string fullPath = item.Prefix.TrimEnd('/');
						string dirName = fullPath.Substring(prefix.Length);
						directoryModel.ChildFolders.Add(new DirectoryViewModel(dirName, "/" + fullPath));
					}
					else if (!FilesToExclude.Contains(Path.GetFileName(item.Blob.Name)))
					{
						BlobItemProperties properties = item.Blob.Properties;
						DateTime lastModified = properties.LastModified.HasValue ? properties.LastModified.Value.DateTime : DateTime.MinValue;
						long length = properties.ContentLength ?? 0;

						FileViewModel fileModel = new FileViewModel(Path.GetFileName(item.Blob.Name), Path.GetExtension(item.Blob.Name).Replace(".", ""), length, lastModified, dir);
						directoryModel.Files.Add(fileModel);
					}
				}

				return directoryModel;
			}
			catch (RequestFailedException e)
			{
				throw new FileException(e.Message, e);
			}
		}

		public string Upload(string destination, IFormFileCollection files)
		{
			try
			{
				BlobContainerClient container = GetBlobContainer();

				string fileName = "";

				// For checking the setting to overwrite existing files
				SiteSettings siteSettings = _settingsService.GetSiteSettings();
				IEnumerable<string> allowedExtensions = siteSettings.AllowedFileTypesList
													.Select(x => x.ToLower());

				for (int i = 0; i < files.Count; i++)
				{
					// Find the file's extension
					IFormFile sourceFile = files[i];
					string sourceFileName = Path.GetFileName(sourceFile.FileName);
					string extension = Path.GetExtension(sourceFileName).Replace(".", "");

					if (!string.IsNullOrEmpty(extension))
						extension = extension.ToLower();

					// Check if it's an allowed extension
					if (allowedExtensions.Contains(extension))
					{
						string filePath = CleanPath(String.Format("/{0}/{1}", destination, sourceFileName));
						BlobClient blob = container.GetBlobClient(filePath);

						// Check if it exists already
						if (!siteSettings.OverwriteExistingFiles && blob.Exists())
						{
							string errorMessage = string.Format(SiteStrings.FileManager_Upload_FileAlreadyExists, sourceFileName);
							throw new FileException(errorMessage, null);
						}

						using (Stream stream = sourceFile.OpenReadStream())
						{
							var options = new BlobUploadOptions()
							{
								HttpHeaders = new BlobHttpHeaders() { ContentType = MimeTypes.GetMimeType(Path.GetExtension(sourceFileName)) }
							};

							blob.Upload(stream, options);
						}

						fileName = sourceFileName;
					}
					else
					{
						string allowedExtensionsCsv = string.Join(",", allowedExtensions);
						string errorMessage = string.Format(SiteStrings.FileManager_Extension_Not_Supported, allowedExtensionsCsv);
						throw new FileException(errorMessage, null);
					}
				}

				return fileName;
			}
			catch (RequestFailedException e)
			{
				throw new FileException(e.Message, e);
			}
		}

		public void WriteResponse(string localPath, string applicationPath, string modifiedSinceHeader, IResponseWrapper responseWrapper)
		{
			string blobPath = localPath;

			try
			{
				if (!string.IsNullOrEmpty(applicationPath) && applicationPath != "/" && blobPath.StartsWith(applicationPath))
					blobPath = blobPath.Substring(applicationPath.Length);

				blobPath = CleanPath(blobPath.Replace(_applicationSettings.AttachmentsRoutePath, String.Empty));

				BlobContainerClient container = GetBlobContainer();
				BlobClient blob = container.GetBlobClient(blobPath);

				if (!blob.Exists())
				{
					Log.Warn("The url {0} (translated to {1}) does not exist on the server.", localPath, blobPath);
					throw new HttpStatusException(404, string.Format("{0} does not exist on the server.", localPath));
				}

				BlobProperties properties = blob.GetProperties().Value;
				responseWrapper.AddStatusCodeForCache(blobPath, modifiedSinceHeader);

				if (responseWrapper.StatusCode != 304)
				{
					responseWrapper.ContentType = string.IsNullOrEmpty(properties.ContentType)
						? MimeTypes.GetMimeType(Path.GetExtension(blobPath))
						: properties.ContentType;

					responseWrapper.BinaryWrite(blob.DownloadContent().Value.Content.ToArray());
				}

				responseWrapper.End();
			}
			catch (RequestFailedException ex)
			{
				Log.Error(ex, "There was a problem opening the file {0}.", localPath);
				throw new HttpStatusException(500, "There was a problem opening the file (see the error logs)");
			}
		}

		private BlobContainerClient GetBlobContainer()
		{
			string connectionString = _applicationSettings.AzureConnectionString;
			if (connectionString.Contains("UseDevelopmentStorage"))
				connectionString = "UseDevelopmentStorage=true";

			var serviceClient = new BlobServiceClient(connectionString);
			BlobContainerClient container = serviceClient.GetBlobContainerClient(_applicationSettings.AzureContainer.ToLower());

			Response<BlobContainerInfo> created = container.CreateIfNotExists(PublicAccessType.BlobContainer);
			return container;
		}

		private static string GetDirectoryPrefix(string path)
		{
			string prefix = CleanPath(path ?? "");
			if (prefix.Length > 0 && !prefix.EndsWith("/"))
				prefix += "/";

			return prefix;
		}

		/// <summary>
		/// Normalises slashes and removes the leading slash, as blob names are relative to the container.
		/// </summary>
		private static string CleanPath(string path)
		{
			path = _multipleSlashRegex.Replace(path, "/");
			return path.TrimStart('/');
		}
	}
}
