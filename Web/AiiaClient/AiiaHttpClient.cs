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

        public AiiaHttpClient(IOptionsMonitor<SiteOptions> options, HttpClient client)
        {
            _httpClient = client;
            _httpClient.BaseAddress = new Uri(options.CurrentValue.Aiia.BaseApiUrl);
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
            AiiaAccessToken? accessTokens = null)
        {
            return CallApi<object, T>(url, null, HttpMethod.Get, accessTokens?.TokenScheme, accessTokens?.Token);
        }

        public Task<TResponse> HttpPost<TRequest, TResponse>(string url,
            TRequest body,
            AiiaAccessToken? accessTokens = null)
        {
            return CallApi<TRequest, TResponse>(url, body, HttpMethod.Post, accessTokens?.TokenScheme, accessTokens?.Token);
        }

        public Task<T> HttpGet<T>(string url,
            AiiaClientSecret clientSecret)
        {
            return CallApi<object, T>(url, null, HttpMethod.Get, "Basic", GenerateBasicAuthorizationHeaderValue(clientSecret));
        }

        public Task<TResponse> HttpPost<TRequest, TResponse>(string url,
            TRequest body,
            AiiaClientSecret clientSecret)
        {
            return CallApi<TRequest, TResponse>(url, body, HttpMethod.Post, "Basic", GenerateBasicAuthorizationHeaderValue(clientSecret));
        }

        private string GenerateBasicAuthorizationHeaderValue(AiiaClientSecret secret)
        {
            var credentials = $"{secret.ClientId}:{secret.Secret}";
            var credentialsByteData = Encoding.GetEncoding("iso-8859-1").GetBytes(credentials);
            var base64Credentials = Convert.ToBase64String(credentialsByteData);
            return $"{base64Credentials}";
        }
    }
}