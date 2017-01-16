using System;
using System.Collections.Generic;
using System.Net.Http;
using AzureBlobFileSystem.Contract;
using AzureBlobFileSystem.Infrastructure;
using AzureBlobFileSystem.Model;
using Newtonsoft.Json;

namespace AzureBlobFileSystem.Implementation
{
    public class AzureOAuthProvider : IAzureOAuthProvider
    {
        private static AuthenticationToken _token;

        private readonly IAuthenticationConfiguration _authenticationConfiguration;

        public AzureOAuthProvider(IAuthenticationConfiguration authenticationConfiguration)
        {
            _authenticationConfiguration = authenticationConfiguration;
        }

        public string GetToken()
        {
            _token = Execute();
            return _token.AccessToken;
        }

        private AuthenticationToken Execute()
        {
            using (var client = new HttpClient())
            {
                var request = BuildRequest();    
                var result = client.SendAsync(request).Result;
                result.EnsureSuccessStatusCode();

                var content = result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<AuthenticationToken>(content);
            }
        }

        private HttpRequestMessage BuildRequest()
        {
            var uri = BuildUri();
            var requestContent = BuildRequestContent();
            var encodedContent = new FormUrlEncodedContent(requestContent);

            return new HttpRequestMessage(HttpMethod.Post, uri) { Content = encodedContent };
        }

        private Uri BuildUri()
        {
            var uriString = string.Format(_authenticationConfiguration.OAuthLoginUrl, _authenticationConfiguration.AzureTenantId);
            return new Uri(uriString);
        }

        private Dictionary<string, string> BuildRequestContent()
        {
            return new Dictionary<string, string>
            {
                ["grant_type"] = _authenticationConfiguration.AzureGrantType,
                ["client_secret"] = _authenticationConfiguration.AzureUserKey,
                ["client_id"] = _authenticationConfiguration.AzureUserId,
                ["resource"] = _authenticationConfiguration.AzureResourceUrl
            };
        }
    }
}
