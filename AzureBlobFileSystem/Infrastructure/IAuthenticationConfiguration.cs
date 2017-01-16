namespace AzureBlobFileSystem.Infrastructure
{
    public interface IAuthenticationConfiguration
    {
        string OAuthLoginUrl { get; }

        string AzureTenantId { get; }

        string AzureUserId { get; }

        string AzureUserKey { get; }

        string AzureResourceUrl { get; }

        string AzureGrantType { get; }
    }
}
