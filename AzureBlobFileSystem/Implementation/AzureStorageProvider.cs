using AzureBlobFileSystem.Interface;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Implementation
{
    public class AzureStorageProvider : IAzureStorageProvider
    {
        private readonly IConfiguration _configuration;

        public AzureStorageProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            StorageAccount = GetStorageAccount();
        }

        public CloudStorageAccount StorageAccount { get; }

        public CloudBlobContainer GetContainer()
        {
            var blobClient = StorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_configuration.ContainerName);
            container.CreateIfNotExists(BlobContainerPublicAccessType.Blob);
            return container;
        }

        public CloudBlockBlob GetBlockBlob(string blobName)
        {
            var container = GetContainer();
            return container.GetBlockBlobReference(blobName);
        }

        private CloudStorageAccount GetStorageAccount()
        {
            return CloudStorageAccount.Parse(_configuration.StorageAccountConnectionString);
        }
    }
}
