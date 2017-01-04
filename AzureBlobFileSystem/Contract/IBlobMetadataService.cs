using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Contract
{
    public interface IBlobMetadataService
    {
        BlobMetadata List(CloudBlob blob);
        bool TrySet(CloudBlockBlob blob, BlobMetadata blobMetadata);
    }
}
