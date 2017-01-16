namespace AzureBlobFileSystem.Infrastructure
{
    public interface IAzureStorageConfiguration
    {
        string StorageAccountConnectionString { get; }

        string ContainerName { get; }
    }
}
