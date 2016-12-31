using System.Collections.Generic;
using AzureBlobFileSystem.Interface;
using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Implementation
{
    public class FileInfoService : IFileInfoService
    {
        private readonly IConfiguration _configuration;

        public FileInfoService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<FileInfo> Build(IEnumerable<IListBlobItem> blobItems, bool includeMetadata)
        {
            var fileInfoItems = new List<FileInfo>();

            foreach (var blobItem in blobItems)
            {
                var cloudBlob = blobItem as CloudBlob;

                if (cloudBlob == null)
                {
                    continue;
                }

                var fileInfo = BuildFileInfo(cloudBlob, includeMetadata);
                fileInfoItems.Add(fileInfo);
            }

            return fileInfoItems;
        }

        private FileInfo BuildFileInfo(CloudBlob blob, bool includeMetadata)
        {
            BlobMetadata metadata = null;

            if (includeMetadata)
            {
                metadata = FetchMetadata(blob);
            }

            return new FileInfo
            {
                Metadata = metadata,
                RelativePath = blob.Name
            };
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
                    .ToString(_configuration.UtcTimeFormat));
            }

            return metadataResult;
        }
    }
}
