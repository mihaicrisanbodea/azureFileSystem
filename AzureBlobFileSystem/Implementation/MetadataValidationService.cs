using System;
using System.Collections.Generic;
using System.Linq;
using AzureBlobFileSystem.Contract;
using AzureBlobFileSystem.Model;

namespace AzureBlobFileSystem.Implementation
{
    public class MetadataValidationService : IMetadataValidationService
    {
        private readonly List<char> _invalidMetadataKeyChars = new List<char> { '-' };

        public void ValidateMetadata(BlobMetadata blobMetadata)
        {
            if (blobMetadata == null)
            {
                return;
            }

            foreach (var key in blobMetadata.Metadata.Keys)
            {
                foreach (var invalidMetadataKeyChar in _invalidMetadataKeyChars)
                {
                    if (key.Contains(invalidMetadataKeyChar))
                    {
                        throw new ArgumentException($"Metadata key cannot contain '{invalidMetadataKeyChar}'.");
                    }
                }
            }
        }
    }
}
