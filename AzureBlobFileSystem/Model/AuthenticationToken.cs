using Newtonsoft.Json;

namespace AzureBlobFileSystem.Model
{
    public class AuthenticationToken
    {
        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
    }
}
