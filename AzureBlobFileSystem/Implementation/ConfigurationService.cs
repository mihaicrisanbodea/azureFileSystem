using System.Configuration;
using AzureBlobFileSystem.Contract;

namespace AzureBlobFileSystem.Implementation
{
    public class ConfigurationService : IConfigurationService
    {
        private const int DefaultBlobListingPageSize = 500;
        private const int MaxBlobListingPageSize = 5000;

        public string StorageAccountConnectionString
            => ConfigurationManager.AppSettings["AbFsStorageAccountConnectionString"];

        public string ContainerName => ConfigurationManager.AppSettings["AbFsContainerName"];

        public string DefaultFileName => ConfigurationManager.AppSettings["AbFsDefaultFileName"];

        public string UtcTimeFormat => ConfigurationManager.AppSettings["UtcTimeFormat"];

        public int BlobListingPageSize => GetBlobListingPageSize();

        private int GetBlobListingPageSize()
        {
            var pageSizeString = ConfigurationManager.AppSettings["BlobListingPageSize"];
            int pageSize;
            if (int.TryParse(pageSizeString, out pageSize))
            {
                return pageSize > MaxBlobListingPageSize ? MaxBlobListingPageSize : pageSize;
            }

            return DefaultBlobListingPageSize;
        }
    }
}
