using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AzureBlobFileSystem.Contract;
using AzureBlobFileSystem.Model;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobFileSystem.Implementation
{
    public class FolderInfoService : IFolderInfoService
    {
        public Dictionary<string, FolderInfo> Build(IEnumerable<IListBlobItem> blobItems)
        {
            var folderInfoDictionary = new Dictionary<string, FolderInfo>();

            foreach (var cloudBlob in blobItems.OfType<CloudBlob>())
            {
                var blobPathChunks = GetPathChunks(cloudBlob.Name);

                if (blobPathChunks.Length <= 1)
                {
                    continue;
                }

                BuildFolderInfo(blobPathChunks, folderInfoDictionary);
            }

            return folderInfoDictionary;
        }

        private static string[] GetPathChunks(string blobName)
        {
            return blobName.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static void BuildFolderInfo(string[] pathChunks, Dictionary<string, FolderInfo> folderInfoDictionary)
        {
            var sb = new StringBuilder();
            var partialDirectoryPath = string.Empty;
            FolderInfo folderInfo;

            for (var i = 0; i < pathChunks.Length - 1; i++)
            {
                sb.AppendFormat("{0}/", pathChunks[i]);
                partialDirectoryPath = sb.ToString();
                var isPathAlreadyProcessed = folderInfoDictionary.TryGetValue(partialDirectoryPath, out folderInfo);
                if (isPathAlreadyProcessed)
                {
                    continue;
                }

                TryUpdateParentFolderCount(partialDirectoryPath, folderInfoDictionary);
                folderInfoDictionary.Add(partialDirectoryPath, new FolderInfo { RelativePath = partialDirectoryPath.TrimEnd('/') });
            }

            sb.Append(pathChunks.Last());
            folderInfoDictionary[partialDirectoryPath].FileCount++;
            folderInfoDictionary[partialDirectoryPath].FileRelativePaths.Add(sb.ToString());
        }

        private static void TryUpdateParentFolderCount(string directoryPath, Dictionary<string, FolderInfo> folderInfoDictionary)
        {
            var cleanPath = directoryPath.TrimEnd('/');
            var lastIndex = cleanPath.LastIndexOf('/') + 1;
            if (lastIndex == 0)
            {
                return;
            }

            var rootPath = cleanPath.Remove(lastIndex);
            FolderInfo folderInfo;

            var rootDirectoryMatch = folderInfoDictionary.TryGetValue(rootPath, out folderInfo);
            if (rootDirectoryMatch)
            {
                folderInfoDictionary[rootPath].FolderCount++;
                folderInfoDictionary[rootPath].FolderRelativePaths.Add(cleanPath);
            }
        }
    }
}
