using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AzureBlobFileSystem.Contract;
using AzureBlobFileSystem.Extensions;
using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;
using FileInfo = AzureBlobFileSystem.Model.FileInfo;

namespace AzureBlobFileSystem.Implementation
{
    public class StorageFileService : IStorageFileService
    {
        private readonly IAzureStorageProvider _azureStorageProvider;
        private readonly IAzureBlobItemService _azureBlobItemService;
        private readonly IAzureCdnService _azureCdnService;
        private readonly IPathValidationService _pathValidationService;
        private readonly IFileInfoService _fileInfoService;
        private readonly IBlobMetadataService _blobMetadataService;

        public StorageFileService(IAzureStorageProvider azureStorageProvider, 
            IAzureBlobItemService azureBlobItemService,
            IAzureCdnService azureCdnService,
            IPathValidationService pathValidationService,
            IFileInfoService fileInfoService, 
            IBlobMetadataService blobMetadataService)
        {
            _azureStorageProvider = azureStorageProvider;
            _azureBlobItemService = azureBlobItemService;
            _azureCdnService = azureCdnService;
            _pathValidationService = pathValidationService;
            _fileInfoService = fileInfoService;
            _blobMetadataService = blobMetadataService;
        }

        public FileInfo Create(string path, BlobMetadata blobMetadata = null, Stream stream = null, bool preLoadToCdn = false)
        {
            _pathValidationService.ValidateNotEmpty(path);
            CloudBlobContainer container = _azureStorageProvider.Container;
            path = container.EnsureFileDoesNotExist(path);

            var blob = container.GetBlockBlobReference(path);
            TrySetContentType(blob, path);
            TryUploadStream(stream, blob, blobMetadata);

            if (preLoadToCdn && stream != null)
            {
                _azureCdnService.LoadAsync(path).GetAwaiter();
            }

            return new FileInfo
            {
                Metadata = blobMetadata,
                RelativePath = path
            };
        }

        public List<FileInfo> List(string prefix, bool firstLevelOnly = false, bool includeMetadata = false)
        {
            var listingDetails = GetListingDetails(includeMetadata);
            var blobItems = _azureBlobItemService.ListBlobsAsync(prefix, listingDetails).Result;
            if (firstLevelOnly)
            {
                blobItems = FilterChildBlobItems(prefix, blobItems).ToList();
            }
            return _fileInfoService.Build(blobItems, includeMetadata);
        }
        
        public async Task CopyAsync(string sourcePath, string destinationPath, bool keepSource = true, bool updateCdn = false)
        {
            var container = _azureStorageProvider.Container;
            await CopyAsync(container, sourcePath, destinationPath, keepSource, updateCdn);
        }

        public async Task CopyAsync(CloudBlobContainer container, string sourcePath, string destinationPath, bool keepSource)
        {
            await CopyAsync(container, sourcePath, destinationPath, keepSource, false);
        }

        public void Delete(string path, bool purgeCdn = false)
        {
            _pathValidationService.ValidateFileExists(path);
            var container = _azureStorageProvider.Container;

            if (!container.FileExists(path))
            {
                return;
            }

            var blob = container.GetBlockBlobReference(path);

            DeleteAsync(blob, purgeCdn).GetAwaiter();
        }

        public async Task DeleteAsync(CloudBlockBlob blob)
        {
            await DeleteAsync(blob, false);
        }

        private async Task DeleteAsync(CloudBlockBlob blob, bool purgeCdn)
        {
            blob.Delete();

            if (purgeCdn)
            {
                await _azureCdnService.PurgeAsync(blob.Name);
            }
        }

        private static BlobListingDetails GetListingDetails(bool includeMetadata)
        {
            return includeMetadata
                ? BlobListingDetails.Metadata
                : BlobListingDetails.None;
        }

        private async Task CopyAsync(CloudBlobContainer container, string sourcePath, string destinationPath, bool keepSource, bool updateCdn)
        {
            _pathValidationService.ValidateFileExists(sourcePath, container);
            destinationPath = container.EnsureFileDoesNotExist(destinationPath);

            var source = container.GetBlockBlobReference(sourcePath);
            var target = container.GetBlockBlobReference(destinationPath);

            await target.StartCopyAsync(source);

            if (updateCdn)
            {
                await _azureCdnService.LoadAsync(destinationPath);
            }

            if (!keepSource)
            {
                source.Delete();
                if (updateCdn)
                {
                    await _azureCdnService.PurgeAsync(sourcePath);
                }
            }
        }

        private IEnumerable<IListBlobItem> FilterChildBlobItems(string prefix, List<IListBlobItem> blobItems)
        {
            prefix = $"{prefix}/";

            foreach (var blobItem in blobItems)
            {
                var cloudBlockBlob = blobItem as CloudBlockBlob;

                if (cloudBlockBlob == null)
                {
                    continue;
                }

                var temp = cloudBlockBlob.Name.Replace(prefix, string.Empty);
                if (temp.IndexOf("/", StringComparison.Ordinal) == -1)
                {
                    yield return blobItem;
                }
            }
        }

        private static void TrySetContentType(CloudBlob blob, string path)
        {
            var contentType = MimeMapping.GetMimeMapping(path);
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                blob.Properties.ContentType = contentType;
            }
        }

        private void TryUploadStream(Stream stream, CloudBlockBlob blob, BlobMetadata blobMetadata)
        {
            if (stream == null)
            {
                blob.UploadFromByteArray(new byte[0], 0, 0);
            }
            else
            {
                _blobMetadataService.TrySet(blob, blobMetadata);
                blob.UploadFromStream(stream);
            }
        }
    }
}