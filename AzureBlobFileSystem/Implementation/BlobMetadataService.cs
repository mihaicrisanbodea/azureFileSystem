using System.Linq;
using AzureBlobFileSystem.Contract;
using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Implementation
{
    public class BlobMetadataService : IBlobMetadataService
    {
        private readonly IBusinessConfiguration _businessConfiguration;

        public BlobMetadataService(IBusinessConfiguration businessConfiguration)
        {
            _businessConfiguration = businessConfiguration;
        }

        public BlobMetadata List(CloudBlob blob)
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
                    .ToString(_businessConfiguration.UtcTimeFormat));
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
