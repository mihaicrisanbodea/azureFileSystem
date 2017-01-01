using System.Linq;
using AzureBlobFileSystem.Interface;
using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Implementation
{
    public class BlobMetadataService : IBlobMetadataService
    {
        private readonly IConfiguration _configuration;

        public BlobMetadataService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public BlobMetadata Get(CloudBlob blob)
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

        public bool TrySet(CloudBlockBlob blob, BlobMetadata blobMetadata)
        {
            if (blobMetadata == null)
            {
                return false;
            }

            var blobMeta = blobMetadata.Metadata.Where(m => !string.IsNullOrWhiteSpace(m.Value));
            var metadataSet = false;

            foreach (var metadata in blobMeta)
            {
                blob.Metadata.Add(metadata.Key, metadata.Value);
                metadataSet = true;
            }

            return metadataSet;
        }
    }
}
