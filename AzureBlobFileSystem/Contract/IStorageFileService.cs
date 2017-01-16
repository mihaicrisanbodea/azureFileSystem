using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;
using FileInfo = AzureBlobFileSystem.Model.FileInfo;

namespace AzureBlobFileSystem.Contract
{
    /// <summary>
    /// Service for handling files in Microsoft Azure blob storage.
    /// </summary>
    public interface IStorageFileService
    {
        /// <summary>
        /// Create a file at the given path and (optionally) add metadata to it.
        /// </summary>
        /// <param name="path">
        /// The path at which the file will be created (including the name of the file).
        /// </param>
        /// <param name="blobMetadata">
        /// The metadata (optional) for the file. It consists of a string-string key-value pair,
        /// accepting the properties to be saved along with the file.
        /// </param>
        /// <param name="stream">
        /// The filestream to be saved.
        /// </param>
        /// <param name="preLoadToCdn">
        /// Optional parameter specifying if the file should be pre loaded to cdn.
        /// Precondition: stream should not be null.
        /// TRUE = update cdn
        /// FALSE = don't update cdn
        /// </param>
        /// <returns>
        /// The information for the file created.
        /// </returns>
        FileInfo Create(string path, BlobMetadata blobMetadata = null, Stream stream = null, bool preLoadToCdn = false);

        /// <summary>
        /// List the information for all the files that start with a given prefix and
        /// (optionally) include the metadata for them.
        /// </summary>
        /// <param name="prefix">
        /// The starting path for the files to list the information for.
        /// </param>
        /// <param name="firstLevelOnly">
        /// Optional parameter specifying if only child files should be returned.
        /// The default behavior is listing the entire tree.
        /// </param>
        /// <param name="includeMetadata">
        /// Optional parameter specifying if the metadata should be included.
        /// </param>
        /// <returns>
        /// The list with the file information(s) for the given path.
        /// </returns>
        List<FileInfo> List(string prefix, bool firstLevelOnly = false, bool includeMetadata = false);

        /// <summary>
        /// Copy the file from a path to another.
        /// It can also be used as a 'MOVE' operation by changing the optional
        /// 'keepSource' parameter.
        /// Asynchronous operation.
        /// </summary>
        /// <param name="sourcePath">
        /// Source path (copy file from)
        /// </param>
        /// <param name="destinationPath">
        /// Destination path (copy file to)
        /// </param>
        /// <param name="keepSource">
        /// Optional parameter specifying whether the source should be deleted or not.
        /// TRUE = don't delete source
        /// FALSE = delete source
        /// </param>
        /// <param name="updateCdn">
        /// Optional parameter specifying whether the cdn should be updated.
        /// TRUE = update cdn
        /// FALSE = don't update cdn
        /// </param>
        Task CopyAsync(string sourcePath, string destinationPath, bool keepSource = true, bool updateCdn = false);

        /// <summary>
        /// Copy the file from a path to another.
        /// It can also be used as a 'MOVE' operation by changing the optional
        /// 'keepSource' parameter.
        /// Asynchronous operation.
        /// </summary>
        /// <param name="container">
        /// The cloud blob container that holds the source file.
        /// </param>
        /// <param name="sourcePath">
        /// Source path (copy file from)
        /// </param>
        /// <param name="destinationPath">
        /// Destination path (copy file to)
        /// </param>
        /// <param name="keepSource">
        /// Parameter specifying whether the source should be deleted or not.
        /// TRUE = don't delete source
        /// FALSE = delete source
        /// </param>
        Task CopyAsync(CloudBlobContainer container, string sourcePath, string destinationPath, bool keepSource);

        /// <summary>
        /// Delete the file from the specified path.
        /// </summary>
        /// <param name="path">
        /// Source path (delete file with path).
        /// </param>
        /// <param name="purgeCdn">
        /// Optional parameter specifying if the file should be purged from the cdn.
        /// TRUE = purge file from cdn
        /// FALSE = don't purge file from cdn
        /// </param>
        void Delete(string path, bool purgeCdn = false);

        /// <summary>
        /// Delete the specified blob.
        /// Asynchronous operation.
        /// </summary>
        /// <param name="blob">
        /// The blob to delete.
        /// </param>
        Task DeleteAsync(CloudBlockBlob blob);
    }
}
