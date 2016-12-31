namespace AzureBlobFileSystem.Interface
{
    public interface IConfiguration
    {
        string StorageAccountConnectionString { get; }

        string ContainerName { get; }

        string DefaultFileName { get; }

        string UtcTimeFormat { get; }

        int BlobListingPageSize { get; }
    }
}
