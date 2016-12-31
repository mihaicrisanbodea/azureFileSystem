using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AzureBlobFileSystem.Extensions;
using AzureBlobFileSystem.Interface;
using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;
using FileInfo = AzureBlobFileSystem.Model.FileInfo;

namespace AzureBlobFileSystem.Implementation
{
    public class StorageFileService : IStorageFileService
    {
        private readonly IAzureStorageProvider _azureStorageProvider;
        private readonly IPathValidationService _pathValidationService;
        private readonly IConfiguration _configuration;
        private readonly IFileInfoService _fileInfoService;

        public StorageFileService(IAzureStorageProvider azureStorageProvider, 
            IPathValidationService pathValidationService, 
            IConfiguration configuration, 
            IFileInfoService fileInfoService)
        {
            _azureStorageProvider = azureStorageProvider;
            _pathValidationService = pathValidationService;
            _configuration = configuration;
            _fileInfoService = fileInfoService;
        }

        public FileInfo Create(string path, BlobMetadata blobMetadata = null, Stream stream = null)
        {
            var container = _azureStorageProvider.GetContainer();
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
            var container = _azureStorageProvider.GetContainer();

            BlobContinuationToken token = null;
            List<FileInfo> fileInfoItems = new List<FileInfo>();

            do
            {
                var listingDetails = includeMetadata
                    ? BlobListingDetails.Metadata
                    : BlobListingDetails.None;

                var blobResultSegment = container.ListBlobsSegmented(prefix, true,
                    listingDetails, _configuration.BlobListingPageSize, token, null, null);

                token = blobResultSegment.ContinuationToken;
                IEnumerable<IListBlobItem> blobItems = blobResultSegment.Results;
                var fileInfoList = _fileInfoService.Build(blobItems, includeMetadata);
                fileInfoItems.AddRange(fileInfoList);
            } while (token != null);

            return fileInfoItems;
        }
        
        public async Task Copy(string path, string newPath, bool keepSource = true)
        {
            var container = _azureStorageProvider.GetContainer();
            await Copy(container, path, newPath, keepSource);
        }

        public async Task Copy(CloudBlobContainer container, string sourcePath, string destinationPath, bool keepSource)
        {
            _pathValidationService.ValidateFileExists(sourcePath, container);
            _pathValidationService.ValidateFileDoesNotExist(destinationPath, container);

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
            var container = _azureStorageProvider.GetContainer();

            if (!container.FileExists(path))
            {
                return;
            }

            var blob = container.GetBlockBlobReference(path);
            blob.Delete();
        }

        private void TrySetContentType(CloudBlob blob, string path)
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
                TrySetMetadata(blob, blobMetadata);
                blob.UploadFromStream(stream);
            }
        }

        private void TrySetMetadata(CloudBlockBlob blob, BlobMetadata blobMetadata)
        {
            if (blobMetadata == null)
            {
                return;
            }

            var blobMeta = blobMetadata.Metadata.Where(m => !string.IsNullOrWhiteSpace(m.Value)).ToList();

            if (!blobMeta.Any())
            {
                return;
            }

            foreach (var metadata in blobMeta)
            {
                blob.Metadata.Add(metadata.Key, metadata.Value);
            }
        }
    }
}