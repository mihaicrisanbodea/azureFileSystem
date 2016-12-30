using System;

namespace AzureBlobFileSystem.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceFirstOccurence(this string value, string oldValue, string newValue)
        {
            var valueLength = value.Length;
            var oldValueLength = oldValue.Length;

            if (oldValueLength > valueLength)
            {
                throw new ArgumentException($"String to replace invalid. Cannot replace {oldValue} in {value}");
            }

            var startIndex = value.IndexOf(oldValue, StringComparison.Ordinal);

            if (startIndex == -1)
            {
                return value;
            }

            var suffix = value.Remove(startIndex, oldValueLength + 1);
            return string.Format("{0}/{1}", newValue, suffix);
        }
    }
}
