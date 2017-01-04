using System.Collections.Generic;
using System.Linq;
using AzureBlobFileSystem.Contract;
using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Implementation
{
    public class FileInfoService : IFileInfoService
    {
        private readonly IBlobMetadataService _blobMetadataService;

        public FileInfoService(IBlobMetadataService blobMetadataService)
        {
            _blobMetadataService = blobMetadataService;
        }

        public List<FileInfo> Build(IEnumerable<IListBlobItem> blobItems, bool includeMetadata)
        {
            return blobItems.OfType<CloudBlob>().Select(cloudBlob => BuildFileInfo(cloudBlob, includeMetadata)).ToList();
        }

        private FileInfo BuildFileInfo(CloudBlob blob, bool includeMetadata)
        {
            return new FileInfo
            {
                Metadata = includeMetadata ? _blobMetadataService.List(blob) : null,
                RelativePath = blob.Name
            };
        }
    }
}
