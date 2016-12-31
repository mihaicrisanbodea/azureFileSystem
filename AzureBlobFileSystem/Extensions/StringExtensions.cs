using System;

namespace AzureBlobFileSystem.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceFirstOccurrence(this string value, string oldValue, string newValue)
        {
            ValidateString(value, nameof(value));
            ValidateString(oldValue, nameof(oldValue));
            ValidateString(newValue, nameof(newValue));

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
            return $"{newValue}/{suffix}";
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
