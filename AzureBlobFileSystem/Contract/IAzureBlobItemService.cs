using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Contract
{
    public interface IAzureBlobItemService
    {
        Task<List<IListBlobItem>> ListBlobsAsync(string prefix, BlobListingDetails listingDetails);
    }
}
