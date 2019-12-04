using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using ViiaSample.Data;
using ViiaSample.Extensions;
using ViiaSample.Models.Viia;

namespace ViiaSample.Services
{
    public interface IViiaService
    {
        Uri GetAuthUri(string userEmail);
        Task<InitiateDataUpdateResponse> InitiateDataUpdate(ClaimsPrincipal principal);
        Task<CodeExchangeResponse> ExchangeCodeForAccessToken(string code);
        Task<CodeExchangeResponse> RefreshAccessToken(string refreshToken);
        Task<IImmutableList<Account>> GetUserAccounts(ClaimsPrincipal principal);

        Task<TransactionsResponse> GetAccountTransactions(ClaimsPrincipal principal, string accountId,
            string pagingToken = null);

        Task<Transaction> GetTransaction(ClaimsPrincipal principal, string accountId, string transactionId);
        Task ProcessWebHookPayload(HttpRequest request);
    }

    public class ViiaService : IViiaService
    {
        private readonly IOptionsMonitor<SiteOptions> _options;
        private readonly ILogger<ViiaService> _logger;
        private readonly Lazy<HttpClient> _httpClient;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public ViiaService(IOptionsMonitor<SiteOptions> options, ILogger<ViiaService> logger,
            ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
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

        public Uri GetAuthUri(string email)
        {
            var connectUrl =
                $"{_options.CurrentValue.Viia.BaseApiUrl}/v1/oauth/connect" +
                $"?client_id={_options.CurrentValue.Viia.ClientId}" +
                "&response_type=code" +
                $"&redirect_uri={_options.CurrentValue.Viia.LoginCallbackUrl}";

            // Adding `email` query parameter will prefill email input in the Viia app
            if (email != null)
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

            return HttpPost<InitiateDataUpdateResponse>("v1/update", requestBody, user.ViiaTokenType,
                user.ViiaAccessToken);
        }

        public async Task<CodeExchangeResponse> ExchangeCodeForAccessToken(string code)
        {
            using (var httpClient = _httpClient.Value)
            {
                var requestUrl = "v1/oauth/token";

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", GenerateBasicAuthorizationHeaderValue());

                var tokenBody = new
                {
                    grant_type = "authorization_code",
                    code,
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

        public async Task<CodeExchangeResponse> RefreshAccessToken(string refreshToken)
        {
            using (var httpClient = _httpClient.Value)
            {
                var requestUrl = "v1/oauth/token";

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", GenerateBasicAuthorizationHeaderValue());

                var tokenBody = new
                {
                    grant_type = "refresh_token",
                    refresh_token = refreshToken,
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

            var result =
                await HttpGet<AccountsResponse>("/v1/accounts", user.ViiaTokenType, user.ViiaAccessToken, principal);
            return result?.Accounts.ToImmutableList();
        }

        public async Task<TransactionsResponse> GetAccountTransactions(ClaimsPrincipal principal,
            string accountId, string pagingToken = null)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
                return null;

            return await HttpPost<TransactionsResponse>($"/v1/accounts/{accountId}/transactions/query", new
            {
                Interval = new Interval(SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(900)),
                    SystemClock.Instance.GetCurrentInstant()),
                PagingToken = pagingToken,
                PageSize = 20
            }, user.ViiaTokenType, user.ViiaAccessToken, principal);
        }

        public async Task<Transaction> GetTransaction(ClaimsPrincipal principal, string accountId, string transactionId)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
                return null;

            return await HttpGet<Transaction>($"/v1/accounts/{accountId}/transactions/{transactionId}",
                user.ViiaTokenType, user.ViiaAccessToken, principal);
        }

        public async Task ProcessWebHookPayload(HttpRequest request)
        {
            request.EnableRewind();
            var payloadString = await ReadRequestBody(request.Body);

            _logger.LogInformation($"Received webhook: {payloadString}");
            // `X-Viia-Signature` is provided optionally if client has configured `WebhookSecret` and is used to verify that webhook was sent by Viia
            var viiaSignature = request.Headers["X-Viia-Signature"];
            if (!VerifySignature(viiaSignature, payloadString))
            {
                _logger.LogWarning("Failed to verify webhook signature");
                return;
            }

            var payload = JObject.Parse(payloadString);

            _logger.LogInformation($"Received webhook payload:\n{payloadString}");
            var data = payload[payload.Properties().First().Name];

            if (data == null)
            {
                _logger.LogInformation("Webhook data not parsed");
                return;
            }


            var consentId = string.IsNullOrEmpty(data["consentId"].Value<string>())
                ? string.Empty
                : data["consentId"].Value<string>();

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

            await _emailService.SendWebhookEmail(user.Email, payloadString);
        }

        private async Task<string> ReadRequestBody(Stream bodyStream)
        {
            string documentContents;
            using (bodyStream)
            {
                using (var readStream = new StreamReader(bodyStream, Encoding.UTF8))
                {
                    documentContents = await readStream.ReadToEndAsync();
                }
            }

            return documentContents;
        }

        // Viia calculates same HMAC hash using the secret only known by the client and Viia
        // If HMAC hashes doesn't mach, it means that the webhook was not sent by Viia
        private bool VerifySignature(string viiaSignature, string payload)
        {
            if (string.IsNullOrWhiteSpace(viiaSignature))
                return true;

            if (string.IsNullOrWhiteSpace(_options.CurrentValue.Viia.WebHookSecret))
                return true;

            var generatedSignature = GenerateHmacSignature(payload, _options.CurrentValue.Viia.WebHookSecret);

            if (generatedSignature != viiaSignature)
            {
                _logger.LogWarning(
                    $"Webhook signatures didn't match. Received:\n{viiaSignature}\nGenerated: {generatedSignature}");
                return false;
            }

            return true;
        }

        // Generate HMAC hash of webhook payload using secret shared with Viia
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

        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Authentication
        // TL;DR:
        // 1. Create string - `{your viia client id}:{your viia client secret}` 
        // 2. Convert that string to byte array using `iso-8859-1` encoding
        // 3. Convert that byte array to base 64
        private string GenerateBasicAuthorizationHeaderValue()
        {
            var credentials = $"{_options.CurrentValue.Viia.ClientId}:{_options.CurrentValue.Viia.ClientSecret}";
            var credentialsByteData = Encoding.GetEncoding("iso-8859-1").GetBytes(credentials);
            var base64Credentials = Convert.ToBase64String(credentialsByteData);
            return $"{base64Credentials}";
        }

        public async Task<T> HttpGet<T>(string url, string accessTokenType = null, string accessToken = null,
            ClaimsPrincipal principal = null, bool isRetry = false)
        {
            try
            {
                return await CallApi<T>(url, null, HttpMethod.Get, accessTokenType, accessToken);
            }
            catch (ViiaClientException e) when (e.StatusCode == HttpStatusCode.Unauthorized && accessToken.IsSet() &&
                                                !isRetry)
            {
                var updatedTokens = await RefreshAccessTokenAndSaveToUser(principal);
                return await HttpGet<T>(url, updatedTokens.TokenType, updatedTokens.AccessToken, principal, true);
            }
        }

        public async Task<T> HttpPost<T>(string url, object body, string accessTokenType = null,
            string accessToken = null, ClaimsPrincipal principal = null, bool isRetry = false)
        {
            try
            {
                return await CallApi<T>(url, body, HttpMethod.Post, accessTokenType, accessToken);
            }
            catch (ViiaClientException e) when (e.StatusCode == HttpStatusCode.Unauthorized && accessToken.IsSet() &&
                                                !isRetry)
            {
                var updatedTokens = await RefreshAccessTokenAndSaveToUser(principal);
                return await HttpPost<T>(url, body, updatedTokens.TokenType, updatedTokens.AccessToken, principal,
                    true);
            }
        }

        private async Task<T> CallApi<T>(string url, object body, HttpMethod method, string accessTokenType = null,
            string accessToken = null)
        {
            HttpResponseMessage result = null;
            string responseContent = null;
            try
            {
                var httpRequestMessage = new HttpRequestMessage(method, url)
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(body,
                            new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)
                                .WithIsoIntervalConverter()),
                        Encoding.UTF8, "application/json")
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
                    responseContent = await result.Content.ReadAsStringAsync();
                    throw new ViiaClientException(url, result.StatusCode);
                }

                responseContent = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent,
                    new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)
                        .WithIsoIntervalConverter());
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

        private async Task<CodeExchangeResponse> RefreshAccessTokenAndSaveToUser(ClaimsPrincipal principal)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return null;
            }

            var result = await RefreshAccessToken(user.ViiaRefreshToken);
            user.ViiaAccessToken = result.AccessToken;
            user.ViiaRefreshToken = result.RefreshToken;
            user.ViiaTokenType = result.TokenType;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return result;
        }

        // Gets the base url of current environment that sample app is running
        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;

            var host = request.Host.ToUriComponent();

            var pathBase = request.PathBase.ToUriComponent();

            return $"{request.Scheme}://{host}{pathBase}";
        }
    }
}