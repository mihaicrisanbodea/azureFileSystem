using System.Collections.Generic;
using System.IO;
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

        public List<FileInfo> List(string prefix, bool includeMetadata = false)
        {
            var container = _azureStorageProvider.Container;

            BlobContinuationToken token = null;
            var fileInfoItems = new List<FileInfo>();

            do
            {
                var listingDetails = GetListingDetails(includeMetadata);
                var blobResultSegment = container.ListBlobsSegmented(prefix, true,
                    listingDetails, _configurationService.BlobListingPageSize, token, null, null);

                token = blobResultSegment.ContinuationToken;
                var blobItems = blobResultSegment.Results;

                var fileInfoList = _fileInfoService.Build(blobItems, includeMetadata);
                fileInfoItems.AddRange(fileInfoList);
            } while (token != null);

            return fileInfoItems;
        }
        
        public async Task Copy(string sourcePath, string destinationPath, bool keepSource = true)
        {
            var container = _azureStorageProvider.Container;
            await Copy(container, sourcePath, destinationPath, keepSource);
        }

        public async Task Copy(CloudBlobContainer container, string sourcePath, string destinationPath, bool keepSource)
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

        private static BlobListingDetails GetListingDetails(bool includeMetadata)
        {
            return includeMetadata
                ? BlobListingDetails.Metadata
                : BlobListingDetails.None;
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