using System.Collections.Generic;
using System.Threading.Tasks;
using AzureBlobFileSystem.Model;

namespace AzureBlobFileSystem.Interface
{
    public interface IStorageFolderService
    {
        void Create(string path);
        List<FolderInfo> List(string prefix);
        Task Copy(string path, string newPath, bool keepSource = true);
        void Delete(string path);
    }
}
