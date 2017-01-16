namespace AzureBlobFileSystem.Contract
{
    public interface IBusinessConfiguration
    {
        string DefaultFileName { get; }

        string UtcTimeFormat { get; }

        int BlobListingPageSize { get; }
    }
}
