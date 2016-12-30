using System.Collections.Generic;

namespace AzureBlobFileSystem.Model
{
    public class BlobMetadata
    {
        public Dictionary<string, string> Metadata { get; private set; }

        public BlobMetadata()
        {
            InitMetadata();
        }

        public void Add(string key, string value)
        {
            string item;
            var itemExists = Metadata.TryGetValue(key, out item);

            if (itemExists)
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    Metadata[key] = value;
                }
            }
            else
            {
                Metadata.Add(key, value);
            }
        }

        private void InitMetadata()
        {
            Metadata = new Dictionary<string, string>
            {
                {"height", string.Empty},
                {"width", string.Empty},
                {"size", string.Empty}
            };
        }
    }
}
