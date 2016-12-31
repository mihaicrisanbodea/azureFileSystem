using AzureBlobFileSystem.Interface;
using System.Configuration;

namespace AzureBlobFileSystem.Implementation
{
    public class Configuration : IConfiguration
    {
        public string StorageAccountConnectionString => ConfigurationManager.AppSettings["StorageAccountConnectionString"];

        public string ContainerName => ConfigurationManager.AppSettings["ContainerName"];

        public string DefaultFileName => "temp.tmp";

        public string UtcTimeFormat => "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

        public int BlobListingPageSize => 500;
    }
}
