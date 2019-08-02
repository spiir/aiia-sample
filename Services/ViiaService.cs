using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ViiaSample.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ViiaSample.Services
{
    public interface IViiaService
    {
        Uri GetAuthUri(ClaimsPrincipal principal, string userEmail);
        Task<InitiateDataUpdateResponse> InitiateDataUpdate(ClaimsPrincipal principal);
        Task<CodeExchangeResponse> ExchangeCodeForAccessToken(string code);
        Task<IImmutableList<Account>> GetUserAccounts(ClaimsPrincipal principal);
        Task<IImmutableList<Transaction>> GetAccountTransactions(ClaimsPrincipal principal, string accountId);
        Task ProcessWebHookPayload(HttpRequest request);

        Task<List<CategorizedTransaction>> GetAccountCategorizedTransactions(ClaimsPrincipal principal,
            string accountId);
    }
    
    public class ViiaService : IViiaService
    {
        private readonly IOptionsMonitor<SiteOptions> _options;
        private readonly ILogger<ViiaService> _logger;
        private readonly Lazy<HttpClient> _httpClient;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public ViiaService(IOptionsMonitor<SiteOptions> options, ILogger<ViiaService> logger, ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _options = options;
            _logger = logger;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _httpClient = new Lazy<HttpClient>(() =>
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(options.CurrentValue.Viia.BaseApiUrl)
                };
                return client;
            });
        }

        public Uri GetAuthUri(ClaimsPrincipal principal, string email)
        {
            var connectUrl =
                $"{_options.CurrentValue.Viia.BaseApiUrl}/v1/oauth/connect" +
                $"?client_id={_options.CurrentValue.Viia.ClientId}" +
                "&response_type=code" +
                $"&redirect_uri={_options.CurrentValue.Viia.LoginCallbackUrl}" +
                "&scope=scope";

            if(email != null)
               connectUrl += $"&email={HttpUtility.UrlEncode(email)}";

            return new Uri(connectUrl);
        }

        public Task<InitiateDataUpdateResponse> InitiateDataUpdate(ClaimsPrincipal principal)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return null;
            }
            var redirectUrl = $"{GetBaseUrl()}/viia/data/{currentUserId}/";
            var requestBody = new InitiateDataUpdateRequest {RedirectUrl = redirectUrl};

            return HttpPost<InitiateDataUpdateResponse>("v1/update", requestBody, user.ViiaTokenType, user.ViiaAccessToken);
        }

        public async Task<CodeExchangeResponse> ExchangeCodeForAccessToken(string code)
        {
            using (var httpClient = CreateApiHttpClient())
            {
                var requestUrl = "v1/oauth/token";
                
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GenerateBasicAuthorizationHeaderValue());
                
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Method = HttpMethod.Post;

                var tokenBody = new
                {
                    grant_type = "authorization_code",
                    code = code,
                    scope = "read",
                    redirect_uri = _options.CurrentValue.Viia.LoginCallbackUrl

                };

                var response = await httpClient.PostAsJsonAsync(requestUrl, tokenBody);
                var content = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to exchange code for tokens");
                }

                var tokenResponse =
                    JsonConvert.DeserializeObject<CodeExchangeResponse>(content);
                return tokenResponse;
            }
        }

        public async Task<IImmutableList<Account>> GetUserAccounts(ClaimsPrincipal principal)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return null;
            }
            var result = await HttpGet<AccountResponse>("/v1/accounts", user.ViiaTokenType, user.ViiaAccessToken);
            return result?.Accounts.ToImmutableList();
        }

        public async Task<IImmutableList<Transaction>> GetAccountTransactions(ClaimsPrincipal principal, string accountId)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return null;
            }

            // TODO paging
            var result = await HttpGet<TransactionResponse>($"/v1/accounts/{accountId}/transactions", user.ViiaTokenType, user.ViiaAccessToken);
            return result?.Transactions.ToImmutableList();
        }
        
        public async Task<List<CategorizedTransaction>> GetAccountCategorizedTransactions(ClaimsPrincipal principal, string accountId)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return null;
            }

            // TODO paging
            var result = await HttpGet<List<CategorizedTransaction>>($"/v1/accounts/{accountId}/transactions/categorized", user.ViiaTokenType, user.ViiaAccessToken);
            return result;
        }

        public async Task ProcessWebHookPayload(HttpRequest request)
        {
            var payloadString = ReadRequestBody(request.Body);
            var viiaSignature = request.Headers["X-Viia-Signature"];
            if (!VerifySignature(viiaSignature, payloadString))
            {
                return;
            }

            var payload = JObject.Parse(payloadString);
            
            _logger.LogInformation($"Received webhook payload:\n{payloadString}");
            var data = payload[payload.Properties().First().Name];
            var consentId = data["ConsentId"].ToString();
            var eventType = data["Event"].ToString();
            
            var user = _dbContext.Users.FirstOrDefault(x => x.ViiaConsentId == consentId);
            if (user == null)
            {
                _logger.LogInformation($"No user found with consent {consentId}");
                // User probably revoked consent
                return;
            }

            if (!user.EmailEnabled)
            {
                _logger.LogInformation("User has disabled email notifications.");
                return;
            }
            switch (eventType)
            {
                case "AccountsUpdated":
                    await _emailService.SendDataUpdateEmail(user.Email, payloadString.ToString());
                    break;
                case "ConnectionUpdateRequired":
                    await _emailService.SendDataUpdateEmail(user.Email, payloadString.ToString());
                    break;
                case "ConsentNeedsUpdate":
                    await _emailService.SendDataUpdateEmail(user.Email, payloadString.ToString());
                    break;
                case "ConsentRevoked":
                    await _emailService.SendDataUpdateEmail(user.Email, payloadString.ToString());
                    break;
                default:
                    await _emailService.SendUnknownWebHookEmail(user.Email, payloadString.ToString());
                    break;
            }
        }

        private string ReadRequestBody(Stream bodyStream)
        {
            string documentContents;
            using (bodyStream)
            {
                using (StreamReader readStream = new StreamReader(bodyStream, Encoding.UTF8))
                {
                    documentContents = readStream.ReadToEnd();
                }
            }
            return documentContents;
        }

        private bool VerifySignature(string viiaSignature, string payload)
        {
            if (string.IsNullOrWhiteSpace(viiaSignature))
                return true;

            if (string.IsNullOrWhiteSpace(_options.CurrentValue.Viia.WebHookSecret))
                return true;
            
            var generatedSignature = GenerateHmacSignature(payload, _options.CurrentValue.Viia.WebHookSecret);

            if (generatedSignature != viiaSignature)
            {
                _logger.LogWarning($"Webhook signatures didn't match. Received:\n{viiaSignature}\nGenerated: {generatedSignature}");
                return false;
            }

            return true;
        }

        private string GenerateHmacSignature(string payload, string secret)
        {
            var encoding = new UTF8Encoding();

            var textBytes = encoding.GetBytes(payload);
            var keyBytes = encoding.GetBytes(secret);

            byte[] hashBytes;

            using (var hash = new HMACSHA256(keyBytes))
            {
                hashBytes = hash.ComputeHash(textBytes);
            }

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private HttpClient CreateApiHttpClient()
        {
            return new HttpClient
            {
                BaseAddress = new Uri(_options.CurrentValue.Viia.BaseApiUrl)
            };
        }

        private string GenerateBasicAuthorizationHeaderValue()
        {
            var credentials = $"{_options.CurrentValue.Viia.ClientId}:{_options.CurrentValue.Viia.ClientSecret}";
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
            return CallApi<T>( url, body, HttpMethod.Post, accessTokenType, accessToken);
        }
        
        private async Task<T> CallApi<T>(string url, object body, HttpMethod method, string accessTokenType = null, string accessToken=null)
        {
            HttpResponseMessage result = null;
            string responseContent = null;
            try
            {
                var httpRequestMessage = new HttpRequestMessage(method, url)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(body),
                        Encoding.UTF8, "application/json"),
                };
                if (accessTokenType != null && accessToken != null)
                {
                    httpRequestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue(accessTokenType, accessToken);
                }

                var sw = Stopwatch.StartNew();
                result = await _httpClient.Value.SendAsync(httpRequestMessage);
                var duration = sw.Elapsed;

                _logger.LogDebug(
                    "Viia request: {RequestUri} {StatusCode} {DurationMilliseconds}ms",
                    result.RequestMessage.RequestUri,
                    result.StatusCode,
                    Math.Round(duration.TotalMilliseconds)
                );

                if (!result.IsSuccessStatusCode)
                {
                    throw new ViiaClientException(url, result.StatusCode);
                }

                responseContent = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (ViiaClientException)
            {
                throw;
            }
            catch (JsonSerializationException e)
            {
                throw new ViiaClientException(url, method, responseContent, e);
            }
            catch (Exception e)
            {
                throw new ViiaClientException(url, method, result, e);
            }
        }

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;

            var host = request.Host.ToUriComponent();

            var pathBase = request.PathBase.ToUriComponent();

            return $"{request.Scheme}://{host}{pathBase}";
        }
    }

    public class AccountResponse
    {
        public List<Account> Accounts { get; set; }
    }

    public class Account
    {
        public AmountModel Available { get; set; }
        public AmountModel Booked { get; set; }
        public string Id { get; set; }
        public AccountProvider Provider { get; set; }
        public string Name { get; set; }
        public AccountNumberViewModel Number { get; set; }
        public string Type { get; set; }
        public DateTime? LastSynchronized { get; set; }
        public string Owner { get; set; }
    }
    
    public class AccountNumberViewModel
    {
        public string BbanType { get; set; }
        public string Bban { get; set; }
        public string Iban { get; set; }
        public BbanParsedViewModel BbanParsed { get; set; }
    }

    public class BbanParsedViewModel
    {
        public string BankCode { get; set; }
        public string AccountNumber { get; set; }
    }
    
    public class AccountProvider
    {
        public string Id { get; set; }
    }

    public class AmountModel
    {
        public string Currency { get; set; }
        public decimal Value { get; set; }
    }

    public class TransactionResponse
    {
        public List<Transaction> Transactions { get; set; }
        public string ContinuationToken { get; set; }
    }
    
    public class Transaction
    {
        public string Id { get; set; }
        public DateTimeOffset? Date { get; set; }
        public AmountModel Balance { get; set; }
        public AmountModel TransactionAmount { get; set; }
        public string Text { get; set; }
        public string OriginalText { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
    }

    public class CategorizedTransaction
    {
        public Transaction Transaction { get; set; }
        public List<string> Categories { get; set; }
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

    public class InitiateDataUpdateResponse
    {
        public UpdateStatus Status { get; set; }
        public string AuthUrl { get; set; }
    }
    public enum UpdateStatus
    {
        AllQueued,
        SupervisedLoginRequired
    }

    public class InitiateDataUpdateRequest
    {
        public string RedirectUrl { get; set; }
    }
}
