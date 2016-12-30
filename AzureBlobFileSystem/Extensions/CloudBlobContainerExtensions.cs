using System;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Extensions
{
    public static class CloudBlobContainerExtensions
    {
        private static string DuplicateExtension = "(2)";

        public static string EnsureDirectoryDoesNotExist(this CloudBlobContainer container, string path)
        {
            var endsWithSlash = path.EndsWith("/");
            var iterationCount = 0;

            while (DirectoryExists(container, path))
            {
                if (iterationCount == 0 && !endsWithSlash)
                {
                    path = $"{path}{DuplicateExtension}";
                }
                else
                {
                    path = path.Substring(0, path.Length - 2);
                    path = $"{path}{DuplicateExtension}";
                }

                iterationCount++;
            }

            return path;
        }

        public static string EnsureFileDoesNotExist(this CloudBlobContainer container, string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var extension = Path.GetExtension(path);
            var rootPath = Path.GetDirectoryName(path);

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

            return container.GetBlockBlobReference(path.Replace("\\", "/")).Exists();
        }
    }
}
