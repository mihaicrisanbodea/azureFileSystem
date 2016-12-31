using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Interface
{
    public interface IPathValidationService
    {
        void ValidateDirectoryExists(string path);

        void ValidateDirectoryDoesNotExist(string path);

        void ValidateNotEmpty(string path);

        void ValidateNotRemovingRoot(string path, bool keepSource);

        void ValidateFileExists(string path, CloudBlobContainer container);

        void ValidateFileDoesNotExist(string path, CloudBlobContainer container);

        void ValidateFileExists(string path);
    }
}
