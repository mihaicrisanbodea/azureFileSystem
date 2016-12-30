namespace AzureBlobFileSystem.Model
{
    public class FileInfo
    {
        public string RelativePath { get; set; }

        public BlobMetadata Metadata { get; set; }
    }
}
