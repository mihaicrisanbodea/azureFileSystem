using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Model
{
    public class FileToCopy
    {
        public CloudBlobContainer Container { get; set; }
        public string FileName { get; set; }
        public string NewFileName { get; set; }
    }
}
