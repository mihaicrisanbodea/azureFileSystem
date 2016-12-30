using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Interface
{
    public interface IAzureStorageProvider
    {
        CloudStorageAccount StorageAccount { get; }
        CloudBlobContainer GetContainer();
        CloudBlockBlob GetBlockBlob(string blobName);
    }
}