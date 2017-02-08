using System.Collections.Generic;
using System.Threading.Tasks;
using AzureBlobFileSystem.Contract;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Implementation
{
    public class AzureBlobItemService : IAzureBlobItemService
    {
        private readonly IAzureStorageProvider _azureStorageProvider;
        private readonly IBusinessConfiguration _businessConfiguration;

        public AzureBlobItemService(IAzureStorageProvider azureStorageProvider, 
            IBusinessConfiguration businessConfiguration)
        {
            _azureStorageProvider = azureStorageProvider;
            _businessConfiguration = businessConfiguration;
        }

        public async Task<List<IListBlobItem>> ListBlobsAsync(string prefix, BlobListingDetails listingDetails)
        {
            var container = _azureStorageProvider.Container;
            var results = new List<IListBlobItem>();
            BlobContinuationToken token = null;

            do
            {
                var blobResultSegment = await container.ListBlobsSegmentedAsync(prefix, true,
                    listingDetails, _businessConfiguration.BlobListingPageSize, token, null, null).ConfigureAwait(false);
                token = blobResultSegment.ContinuationToken;
                results.AddRange(blobResultSegment.Results);
            } while (token != null);

            return results;
        }
    }
}
