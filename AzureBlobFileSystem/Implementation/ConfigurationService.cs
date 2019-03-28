using System.Configuration;
using AzureBlobFileSystem.Contract;
using AzureBlobFileSystem.Infrastructure;

namespace AzureBlobFileSystem.Implementation
{
    public class ConfigurationService : IAzureCdnConfiguration, IAuthenticationConfiguration, IAzureStorageConfiguration, IBusinessConfiguration
    {
        private const int DefaultBlobListingPageSize = 500;
        private const int MaxBlobListingPageSize = 5000;

        public string StorageAccountConnectionString
            => ConfigurationManager.AppSettings["AbFsStorageAccountConnectionString"];

        public string ContainerName => ConfigurationManager.AppSettings["AbFsContainerName"];

        public string DefaultFileName => ConfigurationManager.AppSettings["AbFsDefaultFileName"];

        public string UtcTimeFormat => ConfigurationManager.AppSettings["AbFsUtcTimeFormat"];

        public int BlobListingPageSize => GetBlobListingPageSize();

        public string OAuthLoginUrl => ConfigurationManager.AppSettings["AbFsOAuthLoginUrl"];

        public string AzureTenantId => ConfigurationManager.AppSettings["AbFsAzureTenantId"];

        public string AzureUserId => ConfigurationManager.AppSettings["AbFsAzureUserId"];

        public string AzureUserKey => ConfigurationManager.AppSettings["AbFsAzureUserKey"];

        public string AzureResourceUrl => ConfigurationManager.AppSettings["AbFsAzureResourceUrl"];

        public string AzureGrantType => ConfigurationManager.AppSettings["AbFsAzureGrantType"];

        public string AzureCdnUrl => ConfigurationManager.AppSettings["AbFsAzureCdnUrl"];

        public string AzureSubscriptionId => ConfigurationManager.AppSettings["AbFsAzureSubscriptionId"];

        public string AzureCdnResourceGroupName => ConfigurationManager.AppSettings["AbFsAzureCdnResourceGroupName"];

        public string AzureCdnProfileName => ConfigurationManager.AppSettings["AbFsAzureCdnProfileName"];

        public string AzureCdnEndpointName => ConfigurationManager.AppSettings["AbFsAzureCdnEndpointName"];

        public string ApiVersion => ConfigurationManager.AppSettings["AbFsApiVersion"];

        private static int GetBlobListingPageSize()
        {
            var pageSizeString = ConfigurationManager.AppSettings["AbFsBlobListingPageSize"];
            int pageSize;
            if (int.TryParse(pageSizeString, out pageSize))
            {
                return pageSize > MaxBlobListingPageSize ? MaxBlobListingPageSize : pageSize;
            }

            return DefaultBlobListingPageSize;
        }
    }
}
