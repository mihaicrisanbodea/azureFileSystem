using System.Threading.Tasks;

namespace AzureBlobFileSystem.Contract
{
    public interface IAzureOAuthProvider
    {
        string GetToken();
    }
}
