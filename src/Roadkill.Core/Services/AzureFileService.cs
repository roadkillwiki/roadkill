using System.Collections.Concurrent;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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
using System.Web;
using WebGrease.Css.Extensions;

namespace Roadkill.Core.Services
{
	public class AzureFileService : IFileService
	{
		private readonly ApplicationSettings _applicationSettings;
		private readonly SettingsService _settingsService;
		private static readonly string[] FilesToExclude = new string[] { "emptyfile.txt", "_installtest.txt" }; // installer/publish files

		public AzureFileService(ApplicationSettings applicationSettings, SettingsService settingsService)
		{
			_applicationSettings = applicationSettings;
			_settingsService = settingsService;
		}

		public void Delete(string filePath, string fileName)
		{
			try
			{
				CloudBlobContainer container = GetCloudBlobContainer();
				string path = CleanPath(String.Format("/{0}/{1}", filePath, fileName));
				container.GetBlockBlobReference(path).DeleteIfExists();
			}
			catch (StorageException e)
			{
				throw new FileException(e, "Unable to delete {0} from {1}", fileName, filePath);
			}
		}

		public void DeleteFolder(string folderPath)
		{
			try
			{
				CloudBlobContainer container = GetCloudBlobContainer();
				var azureDirectory = container.GetDirectoryReference(CleanPath(folderPath));
				var files = azureDirectory.ListBlobs()
										  .OfType<CloudBlockBlob>()
										  .Where(b => !FilesToExclude.Contains(Path.GetFileName(b.Name)))
										  .ToList();

				var directories = azureDirectory.ListBlobs()
												.Select(b => b as CloudBlobDirectory)
												.Where(b => b != null)
												.ToList();

				if (files.Count == 0 && directories.Count == 0)
				{
					azureDirectory.ListBlobs().OfType<CloudBlockBlob>().ForEach(b => b.Delete());
				}
				else
				{
					throw new FileException("The folder is not empty.", null);
				}
			}
			catch (StorageException e)
			{
				throw new FileException(e.Message, e);
			}
		}

		public bool CreateFolder(string parentPath, string folderName)
		{
			try
			{
				CloudBlobContainer container = GetCloudBlobContainer();
				string fileName = CleanPath(parentPath + "/" + folderName + "/" + FilesToExclude[0]);
				var blob = container.GetBlockBlobReference(fileName);

				if (!blob.Exists())
				{
					blob.UploadText(String.Empty);
					return true;
				}
				throw new FileException(SiteStrings.FileManager_Error_CreateFolder + " " + folderName, null);
			}
			catch (StorageException e)
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
				CloudBlobContainer container = GetCloudBlobContainer();
				CloudBlobDirectory azureDirectory = container.GetDirectoryReference(dir);
				List<CloudBlobDirectory> directories = azureDirectory.ListBlobs()
																	.Select(b => b as CloudBlobDirectory)
																	.Where(b => b != null)
																	.ToList();

				List<CloudBlockBlob> files = azureDirectory.ListBlobs()
														  .OfType<CloudBlockBlob>()
														  .Where(b => !FilesToExclude.Contains(Path.GetFileName(b.Name)))
														  .ToList();

				foreach (CloudBlobDirectory directory in directories)
				{
					string dirName = directory.Prefix.TrimEnd('/');
					dirName = dirName.Replace(directory.Parent.Prefix, String.Empty);
					DirectoryViewModel childModel = new DirectoryViewModel(dirName, directory.Prefix.TrimEnd('/'));
					directoryModel.ChildFolders.Add(childModel);
				}

				foreach (CloudBlockBlob file in files)
				{
					file.FetchAttributes();
					string filename = file.Name;
					FileViewModel fileModel = new FileViewModel(Path.GetFileName(file.Name), Path.GetExtension(file.Name).Replace(".", ""), file.Properties.Length, ((DateTimeOffset)file.Properties.LastModified).DateTime, dir);
					directoryModel.Files.Add(fileModel);
				}


				return directoryModel;
			}
			catch (StorageException e)
			{
				throw new FileException(e.Message, e);
			}
		}

		public string Upload(string destination, HttpFileCollectionBase files)
		{
			try
			{
				CloudBlobContainer container = GetCloudBlobContainer();

				// Get the allowed files types
				string fileName = "";

				// For checking the setting to overwrite existing files
				SiteSettings siteSettings = _settingsService.GetSiteSettings();
				IEnumerable<string> allowedExtensions = siteSettings.AllowedFileTypesList
													.Select(x => x.ToLower());

				for (int i = 0; i < files.Count; i++)
				{
					// Find the file's extension
					HttpPostedFileBase sourceFile = files[i];
					string extension = Path.GetExtension(sourceFile.FileName).Replace(".", "");

					if (!string.IsNullOrEmpty(extension))
						extension = extension.ToLower();

					// Check if it's an allowed extension
					if (allowedExtensions.Contains(extension))
					{
						string filePath = CleanPath(String.Format("/{0}/{1}", destination, sourceFile.FileName));
						CloudBlockBlob blob = container.GetBlockBlobReference(filePath);

						// Check if it exists on disk already
						if (!siteSettings.OverwriteExistingFiles)
						{
							if (blob.Exists())
							{
								string errorMessage = string.Format(SiteStrings.FileManager_Upload_FileAlreadyExists, sourceFile.FileName);
								throw new FileException(errorMessage, null);
							}
						}

						blob.UploadFromStream(sourceFile.InputStream);
						fileName = sourceFile.FileName;
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
			catch (StorageException e)
			{
				throw new FileException(e.Message, e);
			}
		}

		public void WriteResponse(string localPath, string applicationPath, string modifiedSinceHeader,
			IResponseWrapper responseWrapper, HttpContext context)
		{
			try
			{
				int bufferSize;
				if (!Int32.TryParse(context.Request["bufferSize"], out bufferSize))
				{
					bufferSize = 100 * 1024;
				}

				CloudBlobContainer container = GetCloudBlobContainer();
				string blobPath = CleanPath(localPath.Replace(_applicationSettings.AttachmentsRoutePath, String.Empty));

				// Add leading slash if necessary
				if (blobPath.Contains("/") && !blobPath.StartsWith("/"))
				{
					blobPath = blobPath.Insert(0, "/");
				}

				CloudBlockBlob blob = container.GetBlockBlobReference(blobPath);

				if (blob.Exists())
				{
					Exception downloadException = null;
					bool isDataComplete = false;
					var dataQueue = new BlockingCollection<Tuple<int, byte[]>>();
					var bufferQueue = new BlockingCollection<byte[]>();

					ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
					{
						try
						{
							Stream blobStream = blob.OpenRead();
							int blockLength = 0;

							do
							{
								byte[] blockBuffer = null;
								if (bufferQueue.Count > 0)
								{
									blockBuffer = bufferQueue.Take();
								}
								else
								{
									blockBuffer = new byte[bufferSize];
								}

								blockLength = blobStream.Read(blockBuffer, 0, blockBuffer.Length);

								if (blockLength > 0)
								{
									dataQueue.Add(new Tuple<int, byte[]>(blockLength, blockBuffer));
								}
							} while (blockLength > 0);
						}
						catch (StorageException exception)
						{
							downloadException = exception;
						}
						finally
						{
							isDataComplete = true;
						}
					}));

					blob.FetchAttributes();
					context.Response.ContentType = blob.Properties.ContentType;
					context.Response.Buffer = false;
					context.Response.AddHeader("content-disposition", "attachment; filename=" + Path.GetFileName(blobPath));

					while (context.Response.IsClientConnected && (!isDataComplete || dataQueue.Count > 0))
					{
						if (downloadException != null)
						{
							throw new FileException("The internal blob download failed.", downloadException);
						}

						var data = dataQueue.Take();
						context.Response.OutputStream.Write(data.Item2, 0, data.Item1);
						context.Response.Flush();

						// Put the processed buffer back into the queue so as to minimize memory consumption
						bufferQueue.Add(data.Item2);
					}
				}
				else
				{
					// 404
					Log.Warn("The url {0} (translated to {1}) does not exist on the server.", localPath, blobPath);

					// Throw so the web.config catches it
					throw new HttpException(404, string.Format("{0} does not exist on the server.", localPath));
				}
			}
			catch (FileException ex)
			{
				// 500
				Log.Error(ex, "There was a problem opening the file {0}.", localPath);

				// Throw so the web.config catches it				
				throw new HttpException(500, "There was a problem opening the file (see the error logs)");
			}
		}

		private CloudBlobContainer GetCloudBlobContainer()
		{
			return GetCloudBlobContainer(_applicationSettings.AzureContainer);
		}

		private CloudBlobContainer GetCloudBlobContainer(string container)
		{
			CloudStorageAccount cloudStorageAccount;
			if (_applicationSettings.AzureConnectionString.Contains("UseDevelopmentStorage"))
			{
				cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
			}
			else
			{
				cloudStorageAccount = CloudStorageAccount.Parse(_applicationSettings.AzureConnectionString);
			}

			CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
			CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(container.ToLower());

			if (!cloudBlobContainer.CreateIfNotExists()) 
				return cloudBlobContainer;

			BlobContainerPermissions permissions = cloudBlobContainer.GetPermissions();
			permissions.PublicAccess = BlobContainerPublicAccessType.Container;
			cloudBlobContainer.SetPermissions(permissions);

			return cloudBlobContainer;
		}

		private static string CleanPath(string path)
		{
			string MultipleSlashPattern = @"(\/+|\\+)";
			Regex multipleSlashRegex = new Regex(MultipleSlashPattern);
			path = multipleSlashRegex.Replace(path, "/");

			return path;
		}
	}
}
