using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureBlobFileSystem.Contract;
using AzureBlobFileSystem.Extensions;
using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Implementation
{
    public class StorageFolderService : IStorageFolderService
    {
        private readonly IAzureStorageProvider _azureStorageProvider;
        private readonly IConfigurationService _configurationService;
        private readonly IFolderInfoService _folderInfoService;
        private readonly IPathValidationService _pathValidationService;
        private readonly IStorageFileService _storageFileService;

        public StorageFolderService(IAzureStorageProvider azureStorageProvider,
            IStorageFileService storageFileService,
            IPathValidationService pathValidationService,
            IConfigurationService configurationService,
            IFolderInfoService folderInfoService)
        {
            _azureStorageProvider = azureStorageProvider;
            _storageFileService = storageFileService;
            _pathValidationService = pathValidationService;
            _configurationService = configurationService;
            _folderInfoService = folderInfoService;
        }

        public void Create(string path)
        {
            _pathValidationService.ValidateNotEmpty(path);

            var container = _azureStorageProvider.Container;
            path = container.EnsureDirectoryDoesNotExist(path);
            path = $"{path}/{_configurationService.DefaultFileName}";

            _storageFileService.Create(path);
        }

        public async Task CopyAsync(string sourcePath, string destinationPath, bool keepSource = true)
        {
            _pathValidationService.ValidateNotRemovingRoot(sourcePath, keepSource);

            if (string.IsNullOrEmpty(sourcePath))
            {
                sourcePath = string.Empty;
            }
            else
            {
                _pathValidationService.ValidateDirectoryExists(sourcePath);
            }

            await CopyRecursively(sourcePath, destinationPath, keepSource);
        }

        public void Delete(string path)
        {
            _pathValidationService.ValidateNotEmpty(path);
            _pathValidationService.ValidateDirectoryExists(path);

            DeleteRecursively(path);
        }

        public List<FolderInfo> List(string prefix)
        {
            BlobContinuationToken token = null;
            var blobItems = ListBlobsAsync(prefix).Result;

            var folderInfoItems = _folderInfoService.Build(blobItems);
            return folderInfoItems.Select(s => s.Value).ToList();
        }

        private async Task<List<IListBlobItem>> ListBlobsAsync(string prefix)
        {
            var container = _azureStorageProvider.Container;
            var results = new List<IListBlobItem>();
            BlobContinuationToken token = null;

            do
            {
                var blobResultSegment = await container.ListBlobsSegmentedAsync(prefix, 
                    true, BlobListingDetails.None, _configurationService.BlobListingPageSize,
                    token, null, null);
                token = blobResultSegment.ContinuationToken;
                results.AddRange(blobResultSegment.Results);
            } while (token != null);

            return results;
        }

        private static string GetPath(CloudBlobDirectory cloudBlobDirectory)
        {
            return cloudBlobDirectory.Prefix.TrimEnd('/');
        }

        private async Task CopyRecursively(string sourcePath, string destinationPath, bool keepSource)
        {
            var container = _azureStorageProvider.Container;

            foreach (var blob in container.GetDirectoryReference(sourcePath).ListBlobs())
            {
                var blockBlob = blob as CloudBlockBlob;
                if (blockBlob != null)
                {
                    var fileName = blockBlob.Name;
                    var newFileName = fileName.ReplaceFirstOccurrence(sourcePath, destinationPath);
                    await _storageFileService.CopyAsync(container, fileName, newFileName, keepSource);
                    continue;
                }

                var blobDirectory = blob as CloudBlobDirectory;

                if (blobDirectory != null)
                {
                    var folderPath = GetPath(blobDirectory);
                    var newFolderPathSuffix = folderPath.ReplaceFirstOccurrence(sourcePath, string.Empty);
                    await CopyRecursively(folderPath, $"{destinationPath}{newFolderPathSuffix}", keepSource);
                }
            }
        }

        private void DeleteRecursively(string path)
        {
            var container = _azureStorageProvider.Container;

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