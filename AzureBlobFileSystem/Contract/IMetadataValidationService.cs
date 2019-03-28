using AzureBlobFileSystem.Model;

namespace AzureBlobFileSystem.Contract
{
    public interface IMetadataValidationService
    {
        void ValidateMetadata(BlobMetadata blobMetadata);
    }
}
