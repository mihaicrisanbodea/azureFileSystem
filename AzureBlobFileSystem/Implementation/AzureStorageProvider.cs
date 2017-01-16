using AzureBlobFileSystem.Contract;
using AzureBlobFileSystem.Infrastructure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Implementation
{
    public class AzureStorageProvider : IAzureStorageProvider
    {
        private readonly IAzureStorageConfiguration _azureStorageConfiguration;

        public AzureStorageProvider(IAzureStorageConfiguration azureStorageConfiguration)
        {
            _azureStorageConfiguration = azureStorageConfiguration;
            StorageAccount = GetStorageAccount();
        }

        public CloudStorageAccount StorageAccount { get; }

        public CloudBlobContainer Container => GetContainer();

        public CloudBlockBlob GetBlockBlob(string blobName)
        {
            return Container.GetBlockBlobReference(blobName);
        }

        private CloudBlobContainer GetContainer()
        {
            var blobClient = StorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_azureStorageConfiguration.ContainerName);
            container.CreateIfNotExists(BlobContainerPublicAccessType.Blob);
            return container;
        }

        private CloudStorageAccount GetStorageAccount()
        {
            return CloudStorageAccount.Parse(_azureStorageConfiguration.StorageAccountConnectionString);
        }
    }
}
