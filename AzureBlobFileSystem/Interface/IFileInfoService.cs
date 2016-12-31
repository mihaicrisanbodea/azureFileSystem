using System.Collections.Generic;
using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Interface
{
    public interface IFileInfoService
    {
        List<FileInfo> Build(IEnumerable<IListBlobItem> blobItems, bool includeMetadata);
    }
}
