using System.Configuration;
using AzureBlobFileSystem.Contract;

namespace AzureBlobFileSystem.Implementation
{
    public class ConfigurationService : IConfigurationService
    {
        private const int DefaultBlobListingPageSize = 500;

        public string StorageAccountConnectionString
            => ConfigurationManager.AppSettings["StorageAccountConnectionString"];

        public string ContainerName => ConfigurationManager.AppSettings["ContainerName"];

        public string DefaultFileName => ConfigurationManager.AppSettings["DefaultFileName"];

        public string UtcTimeFormat => ConfigurationManager.AppSettings["UtcTimeFormat"];

        public int BlobListingPageSize => GetBlobListingPageSize();

        private int GetBlobListingPageSize()
        {
            var pageSizeString = ConfigurationManager.AppSettings["BlobListingPageSize"];
            int pageSize;
            if (int.TryParse(pageSizeString, out pageSize))
            {
                return pageSize;
            }

            return DefaultBlobListingPageSize;
        }
    }
}
