using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Extensions
{
    public static class CloudBlobContainerExtensions
    {
        private static string DuplicateExtension = "(2)";

        public static string EnsureDirectoryDoesNotExist(this CloudBlobContainer container, string path)
        {
            var tempPath = path.TrimEnd('/');

            while (DirectoryExists(container, tempPath))
            {
                tempPath = $"{tempPath}{DuplicateExtension}";
            }

            return tempPath;
        }

        public static string EnsureFileDoesNotExist(this CloudBlobContainer container, string path)
        {
            var fileName = path.GetFileNameWithoutExtension();
            var extension = path.GetExtension();
            var rootPath = path.GetDirectoryName();

            while (FileExists(container, path))
            {
                fileName = $"{fileName}{DuplicateExtension}";
                path = $"{rootPath}/{fileName}{extension}";
            }

            return path;
        }

        public static bool DirectoryExists(this CloudBlobContainer container, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path can't be empty");
            }

            return container.GetDirectoryReference(path).ListBlobs().Any();
        }

        public static bool FileExists(this CloudBlobContainer container, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path can't be empty");
            }

            path = path.Replace("\\", "/");
            return container.GetBlockBlobReference(path).Exists();
        }
    }
}
