using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;
using FileInfo = AzureBlobFileSystem.Model.FileInfo;

namespace AzureBlobFileSystem.Interface
{
    public interface IStorageFileService
    {
        FileInfo Create(string path, BlobMetadata blobMetadata = null, Stream stream = null);
        List<FileInfo> List(string prefix, bool includeMetadata = false);
        Task Copy(string path, string newPath, bool keepSource = true);
        Task Copy(CloudBlobContainer container, string sourcePath, string destinationPath, bool keepSource);
        void Delete(string path);
    }
}
