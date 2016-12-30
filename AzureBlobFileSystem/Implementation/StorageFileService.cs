using System;
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
        private const string UtcTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

        public StorageFileService(IAzureStorageProvider azureStorageProvider)
        {
            _azureStorageProvider = azureStorageProvider;
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
                    listingDetails, 500, token, null, null);

                token = blobResultSegment.ContinuationToken;
                IEnumerable<IListBlobItem> blobsList = blobResultSegment.Results;
                var tasks = Parse(blobsList, includeMetadata);
                fileInfoItems.AddRange(tasks);
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
            if (!container.FileExists(sourcePath))
            {
                throw new ArgumentException("File does not exist at path", sourcePath);
            }

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
                
                if (blobMetadata != null)
                {
                    var blobMeta = blobMetadata.Metadata.Where(m => !string.IsNullOrWhiteSpace(m.Value)).ToList();
                    if (blobMeta.Any())
                    {
                        foreach (var metadata in blobMeta)
                        {
                            if (string.IsNullOrWhiteSpace(metadata.Value))
                            {
                                continue;
                            }
                            blob.Metadata.Add(metadata.Key, metadata.Value);
                        }
                    }
                }
                blob.UploadFromStream(stream);
            }
        }

        private List<FileInfo> Parse(IEnumerable<IListBlobItem> blobItems, bool includeMetadata)
        {
            var fileInfoItems = new List<FileInfo>();

            foreach (var blobItem in blobItems)
            {
                var cloudBlob = blobItem as CloudBlob;

                if (cloudBlob == null)
                {
                    continue;
                }

                BlobMetadata metadata = null;

                if (includeMetadata)
                {
                    metadata = FetchMetadata(cloudBlob);
                }

                fileInfoItems.Add(new FileInfo
                {
                    Metadata = metadata,
                    RelativePath = cloudBlob.Name
                });
            }

            return fileInfoItems;
        }

        private BlobMetadata FetchMetadata(CloudBlob blob)
        {
            var metadataResult = new BlobMetadata();

            foreach (var metadata in blob.Metadata)
            {
                metadataResult.Add(metadata.Key, metadata.Value);
            }

            metadataResult.Add("size", blob.Properties.Length.ToString());

            var lastModifiedDate = blob.Properties.LastModified;
            if (lastModifiedDate != null)
            {
                metadataResult.Add("modified", lastModifiedDate.Value.DateTime.ToUniversalTime()
                    .ToString(UtcTimeFormat));
            }

            return metadataResult;
        }
    }
}