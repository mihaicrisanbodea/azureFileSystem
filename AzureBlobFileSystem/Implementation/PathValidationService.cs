using System;
using System.Linq;
using AzureBlobFileSystem.Interface;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Implementation
{
    public class PathValidationService : IPathValidationService
    {
        private readonly IAzureStorageProvider _azureStorageProvider;

        public PathValidationService(IAzureStorageProvider azureStorageProvider)
        {
            _azureStorageProvider = azureStorageProvider;
        }

        public void ValidateDirectoryExists(string path)
        {
            var directoryExists = DirectoryExists(path);
            if (!directoryExists)
            {
                throw new ArgumentException($"Directory does not exist at path {path}");
            }
        }

        public void ValidateDirectoryDoesNotExist(string path)
        {
            var directoryExists = DirectoryExists(path);
            if (directoryExists)
            {
                throw new ArgumentException($"Directory already exists at path {path}");
            }
        }

        public void ValidateFileExists(string path, CloudBlobContainer container)
        {
            if (!container.GetBlockBlobReference(path.Replace("\\", "/")).Exists())
            {
                throw new ArgumentException("File does not exist at path", path);
            }
        }

        public void ValidateFileExists(string path)
        {
            var container = _azureStorageProvider.GetContainer();
            ValidateFileExists(path, container);
        }

        public void ValidateFileDoesNotExist(string path, CloudBlobContainer container)
        {
            if (container.GetBlockBlobReference(path.Replace("\\", "/")).Exists())
            {
                throw new ArgumentException("File exists at path", path);
            }
        }

        public void ValidateNotEmpty(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path can't be empty");
            }
        }

        public void ValidateNotRemovingRoot(string path, bool keepSource)
        {
            if (string.IsNullOrEmpty(path) && !keepSource)
            {
                throw new ArgumentException("Removing root not supported");
            }
        }

        private bool DirectoryExists(string path)
        {
            var container = _azureStorageProvider.GetContainer();
            return container.GetDirectoryReference(path).ListBlobs().Any();
        }
    }
}
