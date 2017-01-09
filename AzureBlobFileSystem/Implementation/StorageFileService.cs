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
        private readonly IPathValidationService _pathValidationService;
        private readonly IConfigurationService _configurationService;
        private readonly IFileInfoService _fileInfoService;
        private readonly IBlobMetadataService _blobMetadataService;

        public StorageFileService(IAzureStorageProvider azureStorageProvider, 
            IPathValidationService pathValidationService, 
            IConfigurationService configurationService, 
            IFileInfoService fileInfoService, 
            IBlobMetadataService blobMetadataService)
        {
            _azureStorageProvider = azureStorageProvider;
            _pathValidationService = pathValidationService;
            _configurationService = configurationService;
            _fileInfoService = fileInfoService;
            _blobMetadataService = blobMetadataService;
        }

        public FileInfo Create(string path, BlobMetadata blobMetadata = null, Stream stream = null)
        {
            _pathValidationService.ValidateNotEmpty(path);
            CloudBlobContainer container = _azureStorageProvider.Container;
            path = container.EnsureFileDoesNotExist(path);

            var blob = container.GetBlockBlobReference(path);
            TrySetContentType(blob, path);
            TryUploadStream(stream, blob, blobMetadata);

            return new FileInfo
            {
                Metadata = blobMetadata,
                RelativePath = path
            };
        }

        public List<FileInfo> List(string prefix, bool firstLevelOnly = false, bool includeMetadata = false)
        {
            var blobItems = ListBlobsAsync(prefix, includeMetadata).Result;
            if (firstLevelOnly)
            {
                blobItems = FilterListChildBlobItems(prefix, blobItems).ToList();
            }
            return _fileInfoService.Build(blobItems, includeMetadata);
        }
        
        public async Task CopyAsync(string sourcePath, string destinationPath, bool keepSource = true)
        {
            var container = _azureStorageProvider.Container;
            await CopyAsync(container, sourcePath, destinationPath, keepSource);
        }

        public async Task CopyAsync(CloudBlobContainer container, string sourcePath, string destinationPath, bool keepSource)
        {
            _pathValidationService.ValidateFileExists(sourcePath, container);
            destinationPath = container.EnsureFileDoesNotExist(destinationPath);

            var source = container.GetBlockBlobReference(sourcePath);
            var target = container.GetBlockBlobReference(destinationPath);

            await target.StartCopyAsync(source);

            if (!keepSource)
            {
                source.Delete();
            }
        }

        public void Delete(string path)
        {
            _pathValidationService.ValidateFileExists(path);
            var container = _azureStorageProvider.Container;

            if (!container.FileExists(path))
            {
                return;
            }

            var blob = container.GetBlockBlobReference(path);
            blob.Delete();
        }

        private async Task<List<IListBlobItem>> ListBlobsAsync(string prefix, bool includeMetadata)
        {
            var container = _azureStorageProvider.Container;
            var results = new List<IListBlobItem>();
            BlobContinuationToken token = null;

            do
            {
                var listingDetails = GetListingDetails(includeMetadata);
                var blobResultSegment = await container.ListBlobsSegmentedAsync(prefix, true,
                    listingDetails, _configurationService.BlobListingPageSize, token, null, null);
                token = blobResultSegment.ContinuationToken;
                results.AddRange(blobResultSegment.Results);
            } while (token != null);

            return results;
        }

        private static BlobListingDetails GetListingDetails(bool includeMetadata)
        {
            return includeMetadata
                ? BlobListingDetails.Metadata
                : BlobListingDetails.None;
        }

        private IEnumerable<IListBlobItem> FilterListChildBlobItems(string prefix, List<IListBlobItem> blobItems)
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