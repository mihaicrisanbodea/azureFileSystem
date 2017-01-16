using System;

namespace AzureBlobFileSystem.Extensions
{
    public static class StringExtensions
    {
        public static string ReplacePathPrefix(this string path, string oldValue, string newValue)
        {
            ValidateString(path, nameof(path));
            ValidateString(oldValue, nameof(oldValue));
            ValidateString(newValue, nameof(newValue));

            var valueLength = path.Length;
            var oldValueLength = oldValue.Length;

            if (oldValueLength > valueLength)
            {
                throw new ArgumentException($"String to replace invalid. Cannot replace {oldValue} in {path}");
            }

            var startIndex = path.IndexOf(oldValue, StringComparison.Ordinal);

            if (startIndex == -1)
            {
                return path;
            }

            var suffix = path.Remove(startIndex, oldValueLength + 1);
            return $"{newValue}/{suffix}";
        }

        public static string GetDirectoryName(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null");
            }

            var lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex == -1)
            {
                throw new ArgumentException($"Path invalid {path}");
            }
            return path.Substring(0, lastSlashIndex);
        }

        public static string GetExtension(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null");
            }

            var lastDotIndex = path.LastIndexOf('.');
            if (lastDotIndex == -1)
            {
                throw new ArgumentException($"Path has no extension: {path}");
            }
            return path.Substring(lastDotIndex);
        }

        public static string GetFileName(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null");
            }

            var lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex == -1)
            {
                throw new ArgumentException($"Path invalid: {path}");
            }

            return path.Substring(lastSlashIndex + 1);
        }

        public static string GetFileNameWithoutExtension(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null");
            }

            var lastDotIndex = path.LastIndexOf('.');
            var lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex == -1 || lastDotIndex == -1 || lastDotIndex < lastSlashIndex)
            {
                throw new ArgumentException($"Path invalid: {path}");
            }

            return path.Substring(lastSlashIndex + 1, path.Length - lastDotIndex);
        }

        private static void ValidateString(string value, string valueName)
        {
            if (value == null)
            {
                throw new ArgumentException(valueName);
            }
        }
    }
}
