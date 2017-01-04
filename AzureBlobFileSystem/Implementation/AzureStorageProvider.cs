using AzureBlobFileSystem.Contract;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Implementation
{
    public class AzureStorageProvider : IAzureStorageProvider
    {
        private readonly IConfigurationService _configurationService;

        public AzureStorageProvider(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
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
            var container = blobClient.GetContainerReference(_configurationService.ContainerName);
            container.CreateIfNotExists(BlobContainerPublicAccessType.Blob);
            return container;
        }

        private CloudStorageAccount GetStorageAccount()
        {
            return CloudStorageAccount.Parse(_configurationService.StorageAccountConnectionString);
        }
    }
}
