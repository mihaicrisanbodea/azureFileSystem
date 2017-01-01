using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Interface
{
    public interface IBlobMetadataService
    {
        BlobMetadata Get(CloudBlob blob);
        bool TrySet(CloudBlockBlob blob, BlobMetadata blobMetadata);
    }
}
