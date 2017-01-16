using System.Threading.Tasks;

namespace AzureBlobFileSystem.Contract
{
    public interface IAzureCdnService
    {
        Task LoadAsync(string filePath);

        Task PurgeAsync(string filePath);
    }
}
