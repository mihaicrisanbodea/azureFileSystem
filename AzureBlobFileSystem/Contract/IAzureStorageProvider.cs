using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Contract
{
    public interface IAzureStorageProvider
    {
        CloudStorageAccount StorageAccount { get; }
        CloudBlobContainer Container { get; }
        CloudBlockBlob GetBlockBlob(string blobName);
    }
}