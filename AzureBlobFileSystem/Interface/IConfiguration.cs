namespace AzureBlobFileSystem.Interface
{
    public interface IConfiguration
    {
        string StorageAccountConnectionString { get; }
        string ContainerName { get; }
    }
}
