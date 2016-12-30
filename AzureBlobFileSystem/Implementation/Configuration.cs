using AzureBlobFileSystem.Interface;
using System.Configuration;

namespace AzureBlobFileSystem.Implementation
{
    public class Configuration : IConfiguration
    {
        public string StorageAccountConnectionString => ConfigurationManager.AppSettings["StorageAccountConnectionString"];

        public string ContainerName => ConfigurationManager.AppSettings["ContainerName"];
    }
}
