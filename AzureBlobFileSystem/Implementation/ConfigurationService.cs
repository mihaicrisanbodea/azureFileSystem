﻿using System.Configuration;
using AzureBlobFileSystem.Contract;

namespace AzureBlobFileSystem.Implementation
{
    public class ConfigurationService : IConfigurationService
    {
        public string StorageAccountConnectionString
            => ConfigurationManager.AppSettings["StorageAccountConnectionString"];

        public string ContainerName => ConfigurationManager.AppSettings["ContainerName"];

        public string DefaultFileName => "temp.tmp";

        public string UtcTimeFormat => "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

        public int BlobListingPageSize => 500;
    }
}