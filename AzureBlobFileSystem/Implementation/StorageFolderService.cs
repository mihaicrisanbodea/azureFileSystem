using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureBlobFileSystem.Extensions;
using AzureBlobFileSystem.Interface;
using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Implementation
{
    public class StorageFolderService : IStorageFolderService
    {
        private readonly IAzureStorageProvider _azureStorageProvider;
        private readonly IStorageFileService _storageFileService;
        private readonly IPathValidationService _pathValidationService;
        private readonly IConfiguration _configuration;

        public StorageFolderService(IAzureStorageProvider azureStorageProvider, 
            IStorageFileService storageFileService, 
            IPathValidationService pathValidationService, 
            IConfiguration configuration)
        {
            _azureStorageProvider = azureStorageProvider;
            _storageFileService = storageFileService;
            _pathValidationService = pathValidationService;
            _configuration = configuration;
        }

        public void Create(string path)
        {
            var container = _azureStorageProvider.GetContainer();
            path = container.EnsureDirectoryDoesNotExist(path).TrimEnd('/');
            path = $"{path}/{_configuration.DefaultFileName}";

            _storageFileService.Create(path);
        }

        public async Task Copy(string path, string newPath, bool keepSource = true)
        {
            _pathValidationService.ValidateNotRemovingRoot(path, keepSource);

            if (string.IsNullOrEmpty(path))
            {
                path = string.Empty;
            }
            else
            {
                _pathValidationService.ValidateDirectoryExists(path);
            }

            _pathValidationService.ValidateDirectoryDoesNotExist(newPath);

            await CopyRecursively(path, newPath, keepSource);
        }

        public void Delete(string path)
        {
            _pathValidationService.ValidateNotEmpty(path);
            _pathValidationService.ValidateDirectoryExists(path);
            
            DeleteRecursively(path);
        }

        public List<FolderInfo> List(string prefix)
        {
            var container = _azureStorageProvider.GetContainer();

            BlobContinuationToken token = null;
            Dictionary<string, FolderInfo> folderInfoItems = new Dictionary<string, FolderInfo>();

            do
            {
                var blobResultSegment = container.ListBlobsSegmented(prefix, true,
                    BlobListingDetails.None, 500, token, null, null);
                token = blobResultSegment.ContinuationToken;
                IEnumerable<IListBlobItem> blobsList = blobResultSegment.Results;

                var folderInfoResult = ProcessBlobListItems(blobsList);

                MergeDictionaries(folderInfoItems, folderInfoResult);

            } while (token != null);

            return folderInfoItems.Select(s => s.Value).ToList();
        }

        private void MergeDictionaries(Dictionary<string, FolderInfo> dictionary1, Dictionary<string, FolderInfo> dictionary2)
        {
            foreach (var kvp in dictionary2)
            {
                FolderInfo folderInfo;
                var key = kvp.Key;
                var value = kvp.Value;
                var keyAlreadyProcessed = dictionary1.TryGetValue(key, out folderInfo);

                if (keyAlreadyProcessed)
                {
                    folderInfo.FileCount += value.FileCount;
                    folderInfo.FolderCount += value.FolderCount;
                    folderInfo.FileRelativePaths.AddRange(value.FileRelativePaths);
                    folderInfo.FolderRelativePaths.AddRange(value.FolderRelativePaths);
                }
                else
                {
                    dictionary1.Add(key, value);
                }
            }
        }

        private Dictionary<string, FolderInfo> ProcessBlobListItems(IEnumerable<IListBlobItem> blobItems)
        {
            var folderInfoDictionary = new Dictionary<string, FolderInfo>();

            foreach (var blobItem in blobItems)
            {
                var cloudBlob = blobItem as CloudBlob;

                if (cloudBlob == null)
                {
                    continue;
                }

                var blobPathChunks = GetPathChunks(cloudBlob.Name);

                if (blobPathChunks.Length <= 1)
                {
                    continue;
                }

                BuildFolderInfo(blobPathChunks, folderInfoDictionary);
            }

            return folderInfoDictionary;
        }

        private string[] GetPathChunks(string blobName)
        {
            return blobName.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
        }

        private void BuildFolderInfo(string [] pathChunks, Dictionary<string, FolderInfo> folderInfoDictionary)
        {
            var sb = new StringBuilder();
            var partialDirectoryPath = string.Empty;
            FolderInfo folderInfo;

            for (var i = 0; i < pathChunks.Length - 1; i++)
            {
                sb.AppendFormat("{0}/", pathChunks[i]);
                partialDirectoryPath = sb.ToString();
                var isPathAlreadyProcessed = folderInfoDictionary.TryGetValue(partialDirectoryPath, out folderInfo);
                if (isPathAlreadyProcessed)
                {
                    continue;
                }

                TryUpdateFolderCount(partialDirectoryPath, folderInfoDictionary);

                folderInfoDictionary.Add(partialDirectoryPath, new FolderInfo { RelativePath = partialDirectoryPath.TrimEnd('/') });
            }

            if (string.IsNullOrWhiteSpace(partialDirectoryPath))
            {
                return;
            }

            sb.Append(pathChunks.Last());
            folderInfoDictionary[partialDirectoryPath].FileCount++;
            folderInfoDictionary[partialDirectoryPath].FileRelativePaths.Add(sb.ToString());
        }

        private void TryUpdateFolderCount(string directoryPath, Dictionary<string, FolderInfo> folderInfoDictionary)
        {
            var cleanPath = directoryPath.TrimEnd('/');
            var lastIndex = cleanPath.LastIndexOf('/') + 1;
            if (lastIndex == 0)
            {
                return;
            }

            var rootPath = cleanPath.Remove(lastIndex);
            FolderInfo folderInfo;

            var rootDirectoryMatch = folderInfoDictionary.TryGetValue(rootPath, out folderInfo);
            if (rootDirectoryMatch)
            {
                folderInfoDictionary[rootPath].FolderCount++;
                folderInfoDictionary[rootPath].FolderRelativePaths.Add(cleanPath);
            }
        }

        private string GetPath(CloudBlobDirectory cloudBlobDirectory)
        {
            return cloudBlobDirectory.Prefix.TrimEnd('/');
        }


        private async Task CopyRecursively(string path, string newPath, bool keepSource)
        {
            var container = _azureStorageProvider.GetContainer();

            foreach (var blob in container.GetDirectoryReference(path).ListBlobs())
            {
                var blockBlob = blob as CloudBlockBlob;
                if (blockBlob != null)
                {
                    var fileName = blockBlob.Name;
                    var newFileName = fileName.ReplaceFirstOccurence(path, newPath);
                    await _storageFileService.Copy(container, fileName, newFileName, keepSource);
                    continue;
                }

                var blobDirectory = blob as CloudBlobDirectory;

                if (blobDirectory != null)
                {
                    var folderPath = GetPath(blobDirectory);
                    var newFolderPathSuffix = folderPath.ReplaceFirstOccurence(path, string.Empty);
                    await CopyRecursively(folderPath, $"{newPath}{newFolderPathSuffix}", keepSource);
                }
            }
        }

        private void DeleteRecursively(string path)
        {
            var container = _azureStorageProvider.GetContainer();

            foreach (var blob in container.GetDirectoryReference(path).ListBlobs())
            {
                var blockBlob = blob as CloudBlockBlob;
                if (blockBlob != null)
                {
                    blockBlob.Delete();
                    continue;
                }

                var directory = blob as CloudBlobDirectory;
                if (directory != null)
                {
                    DeleteRecursively(GetPath(directory));
                }
            }
        }

    }
}