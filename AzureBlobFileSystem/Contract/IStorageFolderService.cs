using System.Collections.Generic;
using System.Threading.Tasks;
using AzureBlobFileSystem.Model;

namespace AzureBlobFileSystem.Contract
{
    /// <summary>
    /// Service for handling folders in Microsoft Azure blob storage.
    /// </summary>
    public interface IStorageFolderService
    {
        /// <summary>
        /// Create a folder at the given path. 
        /// Because Azure storage does not allow empty folders, 
        /// it adds an empty file aswell.
        /// </summary>
        /// <param name="path">
        /// The path relative to the container at which the folder will be 
        /// (including the name of the folder).
        /// </param>
        void Create(string path);

        /// <summary>
        /// List the information for all the folders that start with the given
        /// prefix.
        /// </summary>
        /// <param name="prefix">
        /// The starting path for the folders to list the information for.
        /// </param>
        /// <returns>
        /// The list with the folder information(s) for the given path.
        /// </returns>
        List<FolderInfo> List(string prefix);

        /// <summary>
        /// Copy the files and folders from a path to another.
        /// It can be also used as a 'MOVE' operation by changing the 
        /// optional 'keepSource' parameter.
        /// Asynchronous operation.
        /// </summary>
        /// <param name="sourcePath">
        /// Source path (copy files from)
        /// </param>
        /// <param name="destinationPath">
        /// Destination path (copy files to). This should also contain the source
        /// folder name if this needs to be copied.
        /// </param>
        /// <param name="keepSource">
        /// Optional parameter specifying whether the source should be deleted or not.
        /// TRUE = don't delete source
        /// FALSE = delete source
        /// </param>
        /// <returns></returns>
        Task Copy(string sourcePath, string destinationPath, bool keepSource = true);

        /// <summary>
        /// Delete the files and folders from a specific path.
        /// </summary>
        /// <param name="path">
        /// The path from the root folder to delete (including children).
        /// </param>
        void Delete(string path);
    }
}
