using System.Collections.Generic;
using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Contract
{
    public interface IFolderInfoService
    {
        Dictionary<string, FolderInfo> Build(IEnumerable<IListBlobItem> blobItems);
    }
}
