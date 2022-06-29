#nullable enable
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Aiia.Sample.Services
{
    public class AiiaHttpClient : IAiiaHttpClient
    {
        private readonly HttpClient _httpClient;

        public AiiaHttpClient(IOptionsMonitor<SiteOptions> options)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(options.CurrentValue.Aiia.BaseApiUrl)
            };
        }

        public async Task<TResponse> CallApi<TRequest, TResponse>(string url,
            TRequest body,
            HttpMethod method,
            string? authScheme,
            string? authToken)
        {
            // prepare request
            var httpRequestMessage = new HttpRequestMessage(method, url);

            if (body is not null)
            {
                httpRequestMessage.Content = new StringContent(
                    // TODO: Serializer settings
                    JsonConvert.SerializeObject(body),
                    Encoding.UTF8,
                    "application/json");
            }

            if (authScheme is not null && authToken is not null)
                httpRequestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue(authScheme, authToken);

            // send request
            var result = await _httpClient.SendAsync(httpRequestMessage);

            // process result
            if (!result.IsSuccessStatusCode)
            {
                throw new AiiaClientException(url, result.StatusCode, await result.Content.ReadAsStringAsync());
            }

            var responseContent = await result.Content.ReadAsStringAsync();

            // TODO: Serializer settings
            return JsonConvert.DeserializeObject<TResponse>(responseContent);
        }

        public Task<T> HttpGet<T>(string url,
            AiiaAccessTokens? accessTokens = null)
        {
            return CallApi<object, T>(url, null, HttpMethod.Get, accessTokens?.TokenType, accessTokens?.AccessToken);
        }

        public Task<TResponse> HttpPost<TRequest, TResponse>(string url,
            TRequest body,
            AiiaAccessTokens? accessTokens = null)
        {
            return CallApi<TRequest, TResponse>(url, body, HttpMethod.Post, accessTokens?.TokenType, accessTokens?.AccessToken);
        }

        public Task<T> HttpGet<T>(string url,
            AiiaClientSecrets clientSecrets)
        {
            return CallApi<object, T>(url, null, HttpMethod.Get, "Basic", GenerateBasicAuthorizationHeaderValue(clientSecrets));
        }

        public Task<TResponse> HttpPost<TRequest, TResponse>(string url,
            TRequest body,
            AiiaClientSecrets clientSecrets)
        {
            return CallApi<TRequest, TResponse>(url, body, HttpMethod.Post, "Basic", GenerateBasicAuthorizationHeaderValue(clientSecrets));
        }

        private string GenerateBasicAuthorizationHeaderValue(AiiaClientSecrets secrets)
        {
            var credentials = $"{secrets.ClientId}:{secrets.Secret}";
            var credentialsByteData = Encoding.GetEncoding("iso-8859-1").GetBytes(credentials);
            var base64Credentials = Convert.ToBase64String(credentialsByteData);
            return $"{base64Credentials}";
        }
    }
}