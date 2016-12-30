using System.Collections.Generic;

namespace AzureBlobFileSystem.Model
{
    public class FolderInfo
    {
        public string RelativePath { get; set; }

        public int FolderCount { get; set; }

        public int FileCount { get; set; }

        public List<string> FileRelativePaths { get; set; } = new List<string>();

        public List<string> FolderRelativePaths { get; set; } = new List<string>();
    }
}
