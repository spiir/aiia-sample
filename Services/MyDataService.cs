using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyDataSample.Data;
using Newtonsoft.Json;

namespace MyDataSample.Services
{
    public interface IMyDataService
    {
        Uri GetAuthUri(ClaimsPrincipal principal);
        Task<CodeExchangeResponse> ExchangeCodeForAccessToken(string code);
        Task<IEnumerable<Account>> GetUserAccounts(ClaimsPrincipal principal);
        Task<IEnumerable<Transaction>> GetAccountTransactions(ClaimsPrincipal principal, string accountId);
    }
    
    public class MyDataService : IMyDataService
    {
        private readonly IOptionsMonitor<SiteOptions> _options;
        private readonly ILogger<MyDataService> _logger;
        private readonly Lazy<HttpClient> _httpClient;
        private readonly ApplicationDbContext _dbContext;
        public MyDataService(IOptionsMonitor<SiteOptions> options, ILogger<MyDataService> logger, ApplicationDbContext dbContext)
        {
            _options = options;
            _logger = logger;
            _dbContext = dbContext;
            _httpClient = new Lazy<HttpClient>(() =>
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(options.CurrentValue.MyData.BaseApiUrl)
                };
                return client;
            });
        }

        public Uri GetAuthUri(ClaimsPrincipal principal)
        {
            var connectUrl =
                $"{_options.CurrentValue.MyData.BaseApiUrl}/oauth/connect" +
                $"?client_id={_options.CurrentValue.MyData.ClientId}" +
                $"&response_type=code" +
                $"&redirect_uri={_options.CurrentValue.MyData.LoginCallbackUrl}" +
                $"&scope=scope";
            return new Uri(connectUrl);
        }

        public async Task<CodeExchangeResponse> ExchangeCodeForAccessToken(string code)
        {
            using (var httpClient = CreateApiHttpClient())
            {
                var requestUrl = QueryHelpers.AddQueryString("oauth/token",
                    new Dictionary<string, string>()
                    {
                        {"grant_type", "Authorization"},
                        {"code", code},
                        {"scope", "read"},
                        {"redirect_uri", _options.CurrentValue.MyData.LoginCallbackUrl}
                    });
                
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", GenerateBasicAuthorizationHeaderValue());
                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to exchange code for tokens");
                }

                var tokenResponse =
                    JsonConvert.DeserializeObject<CodeExchangeResponse>(await response.Content.ReadAsStringAsync());
                return tokenResponse;
            }
        }

        public async Task<IEnumerable<Account>> GetUserAccounts(ClaimsPrincipal principal)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return null;
            }

            return await HttpGet<List<Account>>("/accounts", user.MyDataTokenType, user.MyDataAccessToken);
        }

        public async Task<IEnumerable<Transaction>> GetAccountTransactions(ClaimsPrincipal principal, string accountId)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return null;
            }

            return await HttpGet<List<Transaction>>($"/accounts/{accountId}/transactions", user.MyDataTokenType, user.MyDataAccessToken);
        }

        private HttpClient CreateApiHttpClient()
        {
            return new HttpClient
            {
                BaseAddress = new Uri(_options.CurrentValue.MyData.BaseApiUrl)
            };
        }

        private string GenerateBasicAuthorizationHeaderValue()
        {
            var credentials = $"{_options.CurrentValue.MyData.ClientId}:{_options.CurrentValue.MyData.ClientSecret}";
            var credentialsByteData = Encoding.GetEncoding("iso-8859-1").GetBytes(credentials);
            var base64Credentials = Convert.ToBase64String(credentialsByteData);
            return $"{base64Credentials}";
        }
        
        public Task<T> HttpGet<T>(string url, string accessTokenType = null, string accessToken=null)
        {
            return CallApi<T>(url, null, HttpMethod.Get, accessTokenType, accessToken);
        }

        public Task<T> HttpPost<T>(string url, object body, string accessTokenType = null, string accessToken=null)
        {
            return CallApi<T>( url, body, HttpMethod.Post, accessToken);
        }
        
        private async Task<T> CallApi<T>(string url, object body, HttpMethod method, string accessTokenType = null, string accessToken=null)
        {
            try
            {
                var httpRequestMessage = new HttpRequestMessage(method, url)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(body),
                        Encoding.UTF8, "application/json"),
                };
                if (accessTokenType != null && accessToken != null)
                {
                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(accessTokenType, accessToken);
                }
                var sw = Stopwatch.StartNew();
                var result = await _httpClient.Value.SendAsync(httpRequestMessage);
                var duration = sw.Elapsed;

                _logger.LogDebug(
                    "MyData request: {RequestUri} {StatusCode} {DurationMilliseconds}ms",
                    result.RequestMessage.RequestUri,
                    result.StatusCode,
                    Math.Round(duration.TotalMilliseconds)
                );

                if (!result.IsSuccessStatusCode)
                {
                    throw new MyDataClientException(url, result.StatusCode);
                }

                var responseContent = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (MyDataClientException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new MyDataClientException(url, method, e);
            }
        }
    }
    
    
    public class Account
    {
        public string Id { get; set; }
        public string ProviderId { get; set; }
        public string Name { get; set; }
        public BankNumber Number { get; set; }
        public string BookedBalance { get; set; }
        public string AvailableBalance { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
        public bool IsPaymentAccount { get; set; }
    }

    public class BankNumber
    {
        public string BbanType { get; set; }
        public string Bban { get; set; }
        public string Iban { get; set; }
        public BbanParsed BbanParsed { get; set; }
    }

    public class BbanParsed
    {
        public string BankCode { get; set; }
        public string AccountNumber { get; set; }
    }

    public class Transaction
    {
        public string Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public string Text { get; set; }
        public string OriginalText { get; set; }
        public int Amount { get; set; }
        public string Type { get; set; }
        public string Currency { get; set; }
        public string State { get; set; }
    }

    public class CodeExchangeResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}