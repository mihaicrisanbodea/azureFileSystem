using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AzureBlobFileSystem.Contract;
using AzureBlobFileSystem.Infrastructure;

namespace AzureBlobFileSystem.Implementation
{
    public class AzureCdnService : IAzureCdnService
    {
        private readonly IAzureCdnConfiguration _azureCdnConfiguration;
        private readonly IAzureOAuthProvider _oAuthProvider;
        private const string _loadOperationName = "load";
        private const string _purgeOperationName = "purge";

        public AzureCdnService(IAzureCdnConfiguration azureCdnConfiguration, 
            IAzureOAuthProvider oAuthProvider)
        {
            _azureCdnConfiguration = azureCdnConfiguration;
            _oAuthProvider = oAuthProvider;
        }

        public async Task LoadAsync(string filePath)
        {
            if (!filePath.StartsWith("/"))
            {
                filePath = $"/{filePath}";
            }

            await ExecuteAsync(_loadOperationName, filePath);
        }

        public async Task PurgeAsync(string filePath)
        {
            if (!filePath.StartsWith("/"))
            {
                filePath = $"/{filePath}";
            }

            await ExecuteAsync(_purgeOperationName, filePath);
        }

        private async Task ExecuteAsync(string operationName, string file)
        {
            using (var client = new HttpClient())
            {
                var request = BuildRequest(operationName, file);
                var result = await client.SendAsync(request);
                result.EnsureSuccessStatusCode();
            }
        }

        private HttpRequestMessage BuildRequest(string operationName, string file)
        {
            var uri = BuildUri(operationName);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var token = _oAuthProvider.GetToken();

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent($"{{\"ContentPaths\":[\"{file}\"]}}", Encoding.UTF8, "application/json");
            return request;
        }

        private Uri BuildUri(string operationName)
        {
            return new Uri(string.Format(_azureCdnConfiguration.AzureCdnUrl,
                _azureCdnConfiguration.AzureSubscriptionId,
                _azureCdnConfiguration.AzureCdnResourceGroupName,
                _azureCdnConfiguration.AzureCdnProfileName,
                _azureCdnConfiguration.AzureCdnEndpointName,
                operationName,
                _azureCdnConfiguration.ApiVersion));
        }
    }
}
