namespace AzureBlobFileSystem.Contract
{
    public interface IConfigurationService
    {
        string StorageAccountConnectionString { get; }

        string ContainerName { get; }

        string DefaultFileName { get; }

        string UtcTimeFormat { get; }

        int BlobListingPageSize { get; }
    }
}
