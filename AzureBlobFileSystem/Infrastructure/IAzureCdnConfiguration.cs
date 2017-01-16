namespace AzureBlobFileSystem.Infrastructure
{
    public interface IAzureCdnConfiguration
    {
        string AzureCdnUrl { get; }

        string AzureSubscriptionId { get; }

        string AzureCdnResourceGroupName { get; }

        string AzureCdnProfileName { get; }

        string AzureCdnEndpointName { get; }

        string ApiVersion { get; }
    }
}
