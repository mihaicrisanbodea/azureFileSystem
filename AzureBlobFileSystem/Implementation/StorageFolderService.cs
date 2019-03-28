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
        private readonly IAzureBlobItemService _azureBlobItemService;
        private readonly IBusinessConfiguration _businessConfiguration;
        private readonly IFolderInfoService _folderInfoService;
        private readonly IPathValidationService _pathValidationService;
        private readonly IStorageFileService _storageFileService;
        private readonly IAzureCdnService _azureCdnService;

        public StorageFolderService(IAzureStorageProvider azureStorageProvider,
            IAzureBlobItemService azureBlobItemService,
            IStorageFileService storageFileService,
            IPathValidationService pathValidationService,
            IBusinessConfiguration businessConfiguration,
            IFolderInfoService folderInfoService, 
            IAzureCdnService azureCdnService)
        {
            _azureStorageProvider = azureStorageProvider;
            _azureBlobItemService = azureBlobItemService;
            _storageFileService = storageFileService;
            _pathValidationService = pathValidationService;
            _businessConfiguration = businessConfiguration;
            _folderInfoService = folderInfoService;
            _azureCdnService = azureCdnService;
        }

        public void Create(string path)
        {
            _pathValidationService.ValidateNotEmpty(path);

            var container = _azureStorageProvider.Container;
            path = container.EnsureDirectoryDoesNotExist(path);
            path = $"{path}/{_businessConfiguration.DefaultFileName}";

            _storageFileService.Create(path);
        }

        public void Copy(string sourcePath, string destinationPath, bool keepSource = true, bool updateCdn = false)
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

            var copyStructure = GetCopyStructure(sourcePath, destinationPath).ToList();

            CopyFiles(copyStructure, keepSource, updateCdn);
        }

        public void Delete(string path, bool purgeCdn = false)
        {
            _pathValidationService.ValidateNotEmpty(path);
            _pathValidationService.ValidateDirectoryExists(path);

            var deleteStructure = GetDeleteStructure(path).ToList();

            DeleteFiles(deleteStructure, purgeCdn);
        }

        public List<FolderInfo> List(string prefix)
        {
            BlobContinuationToken token = null;
            var blobItems = _azureBlobItemService.ListBlobsAsync(prefix, BlobListingDetails.None).Result;

            var folderInfoItems = _folderInfoService.Build(blobItems);
            return folderInfoItems.Select(s => s.Value).ToList();
        }
        
        private IEnumerable<FileToCopy> GetCopyStructure(string sourcePath, string destinationPath)
        {
            var container = _azureStorageProvider.Container;

            foreach (var blob in container.GetDirectoryReference(sourcePath).ListBlobs())
            {
                if (blob is CloudBlockBlob blockBlob)
                {
                    var fileName = blockBlob.Name;
                    var newFileName = fileName.ReplacePathPrefix(sourcePath, destinationPath);
                    yield return new FileToCopy
                    {
                        FileName = fileName,
                        NewFileName = newFileName,
                        Container = container
                    };
                    continue;
                }

                if (blob is CloudBlobDirectory blobDirectory)
                {
                    var folderPath = GetPath(blobDirectory);
                    var newFolderPathSuffix = folderPath.ReplacePathPrefix(sourcePath, string.Empty);
                    foreach (
                        var fileToCopy in
                        GetCopyStructure(folderPath, $"{destinationPath}{newFolderPathSuffix}"))
                    {
                        yield return fileToCopy;
                    }
                }
            }
        }

        private void DeleteFiles(List<CloudBlockBlob> deleteStructure, bool purgeCdn)
        {
            var deleteTasks = new List<Task>();
            var purgeCdnTasks = new List<Task>();

            foreach (var blob in deleteStructure)
            {
                deleteTasks.Add(
                    Task.Factory.StartNew(() => _storageFileService.DeleteAsync(blob).ConfigureAwait(false)));
                if (purgeCdn)
                {
                    purgeCdnTasks.Add(
                        Task.Factory.StartNew(() => _azureCdnService.PurgeAsync(blob.Name).ConfigureAwait(false)));
                }
            }

            Task.WhenAll(deleteTasks.ToArray());
            Task.WhenAll(purgeCdnTasks.ToArray());
        }

        private void CopyFiles(List<FileToCopy> copyStructure, bool keepSource, bool updateCdn)
        {
            var createTasks = new List<Task>();
            var updateCdnTasks = new List<Task>();

            foreach (var fileToCopy in copyStructure)
            {
                createTasks.Add(
                    Task.Factory.StartNew(() => CopyFile(fileToCopy, keepSource).ConfigureAwait(false)));
                if (updateCdn)
                {
                    updateCdnTasks.Add(
                        Task.Factory.StartNew(
                            () => _azureCdnService.LoadAsync(fileToCopy.FileName).ConfigureAwait(false)));
                    if (!keepSource)
                    {
                        updateCdnTasks.Add(
                            Task.Factory.StartNew(
                                () => _azureCdnService.PurgeAsync(fileToCopy.FileName).ConfigureAwait(false)));
                    }
                }
            }

            Task.WhenAll(createTasks.ToArray());
            Task.WhenAll(updateCdnTasks.ToArray());
        }

        private async Task CopyFile(FileToCopy fileToCopy, bool keepSource)
        {
            await _storageFileService.CopyAsync(fileToCopy.Container,
                fileToCopy.FileName,
                fileToCopy.NewFileName,
                keepSource);
        }

        private IEnumerable<CloudBlockBlob> GetDeleteStructure(string path)
        {
            var container = _azureStorageProvider.Container;

            foreach (var blob in container.GetDirectoryReference(path).ListBlobs())
            {
                if (blob is CloudBlockBlob blockBlob)
                {
                    yield return blockBlob;
                    continue;
                }

                var directory = blob as CloudBlobDirectory;
                if (directory == null)
                {
                    continue;
                }

                foreach (var cloudBlockBlob in GetDeleteStructure(GetPath(directory)))
                {
                    yield return cloudBlockBlob;
                }
            }
        }

        private static string GetPath(CloudBlobDirectory cloudBlobDirectory)
        {
            return cloudBlobDirectory.Prefix.TrimEnd('/');
        }
    }
}