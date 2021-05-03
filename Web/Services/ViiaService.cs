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
using Aiia.Sample.Data;
using Aiia.Sample.Exceptions;
using Aiia.Sample.Extensions;
using Aiia.Sample.Models;
using Aiia.Sample.Models.Aiia;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Aiia.Sample.Services
{
    public interface IAiiaService
    {
        Task<bool?> AllAccountsSelected(ClaimsPrincipal principal);
        Task<CreatePaymentResponse> CreateInboundPayment(ClaimsPrincipal user, CreatePaymentRequestViewModel body);

        Task<CreatePaymentResponse> CreateOutboundPayment(ClaimsPrincipal principal,
                                                          CreatePaymentRequestViewModel request);
        Task<CreatePaymentResponseV2> CreateOutboundPaymentV2(ClaimsPrincipal principal,
            CreatePaymentRequestViewModelV2 requestViewModel);
        Task<CreatePaymentAuthorizationResponse> CreatePaymentAuthorization(ClaimsPrincipal principal,
            CreatePaymentAuthorizationRequestViewModel requestViewModel);

        Task<CodeExchangeResponse> ExchangeCodeForAccessToken(string code);

        Task<TransactionsResponse> GetAccountTransactions(ClaimsPrincipal principal,
                                                          string accountId,
                                                          TransactionQueryRequestViewModel queryRequest = null);

        Uri GetAuthUri(string userEmail, bool oneTime = false);
        Task<InboundPayment> GetInboundPayment(ClaimsPrincipal principal, string accountId, string paymentId);
        Task<OutboundPayment> GetOutboundPayment(ClaimsPrincipal principal, string accountId, string paymentId);
        Task<PaymentAuthorization> GetPaymentAuthorization(ClaimsPrincipal principal, string accountId, string authorizationId);
        Task<PaymentsResponse> GetPayments(ClaimsPrincipal principal);
        Task<ImmutableList<BankProvider>> GetProviders();
        Task<IImmutableList<Account>> GetUserAccounts(ClaimsPrincipal principal);
        Task<InitiateDataUpdateResponse> InitiateDataUpdate(ClaimsPrincipal principal);

        Task ProcessWebHookPayload(HttpRequest request);
        Task<CodeExchangeResponse> RefreshAccessToken(string refreshToken);
    }

    public class AiiaService : IAiiaService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly Lazy<HttpClient> _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AiiaService> _logger;
        private readonly IOptionsMonitor<SiteOptions> _options;

        public AiiaService(IOptionsMonitor<SiteOptions> options,
                           ILogger<AiiaService> logger,
                           ApplicationDbContext dbContext,
                           IHttpContextAccessor httpContextAccessor,
                           IEmailService emailService)
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
                                                                    BaseAddress = new Uri(options.CurrentValue.Aiia.BaseApiUrl)
                                                                };
                                                   return client;
                                               });
        }

        public async Task<bool?> AllAccountsSelected(ClaimsPrincipal principal)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return false;
            }

            try
            {
                var response = await HttpGet<AllAccountSelectedResponse>(
                                                                         $"v1/consent/{user.AiiaConsentId}/all-accounts-selected",
                                                                         user.AiiaTokenType,
                                                                         user.AiiaAccessToken);

                return response.AllAccountsSelected;
            }
            catch (AiiaClientException e) when (e.StatusCode == HttpStatusCode.Forbidden)
            {
                return null;
            }
        }

        public async Task<CreatePaymentResponse> CreateInboundPayment(ClaimsPrincipal principal,
                                                                      CreatePaymentRequestViewModel request)
        {
            {
                var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
                if (user == null)
                {
                    throw new UserNotFoundException();
                }

                var paymentRequest = new CreateInboundPaymentRequest
                                     {
                                         Culture = request.Culture,
                                         RedirectUrl = GetPaymentRedirectUrl(),
                                         Payment = new InboundPaymentRequest
                                                   {
                                                       Amount = new PaymentAmountRequest
                                                                {
                                                                    Value = request.Amount
                                                                },
                                                   }
                                     };

                return await CallApi<CreatePaymentResponse>($"v1/accounts/{request.SourceAccountId}/payments/inbound",
                                                            paymentRequest,
                                                            HttpMethod.Post,
                                                            user.AiiaTokenType,
                                                            user.AiiaAccessToken);
            }
        }

        public async Task<CreatePaymentResponse> CreateOutboundPayment(ClaimsPrincipal principal,
                                                                       CreatePaymentRequestViewModel request)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            var paymentRequest = new CreateOutboundPaymentRequest
                                 {
                                     Culture = request.Culture,
                                     RedirectUrl = GetPaymentRedirectUrl(),
                                     Payment = new PaymentRequest
                                               {
                                                   Message = request.message,
                                                   TransactionText = request.TransactionText,
                                                   Amount = new PaymentAmountRequest
                                                            {
                                                                Value = request.Amount
                                                            },
                                                   Destination = new PaymentDestinationRequest(),
                                                   PaymentMethod = request.PaymentMethod,
                                               },
                                 };

            paymentRequest.Payment.Destination.RecipientFullname = request.RecipientFullname;
            
            

            if (!string.IsNullOrWhiteSpace(request.Iban))
            {
                paymentRequest.Payment.Destination.IBan = request.Iban;
            }
            else if(!string.IsNullOrWhiteSpace(request.BbanAccountNumber))
            {
                paymentRequest.Payment.Destination.BBan = new PaymentBBanRequest
                                                          {
                                                              BankCode = request.BbanBankCode,
                                                              AccountNumber = request.BbanAccountNumber
                                                          };
            }
            else
            {
                paymentRequest.Payment.Destination.InpaymentForm = new PaymentInpaymentFormRequest
                {
                    Type = request.InpaymentFormType,
                    CreditorNumber = request.InpaymentFormCreditorNumber
                };
            }

            return await CallApi<CreatePaymentResponse>($"v1/accounts/{request.SourceAccountId}/payments/outbound",
                                                        paymentRequest,
                                                        HttpMethod.Post,
                                                        user.AiiaTokenType,
                                                        user.AiiaAccessToken);
        }

        public async Task<CreatePaymentResponseV2> CreateOutboundPaymentV2(ClaimsPrincipal principal, CreatePaymentRequestViewModelV2 request)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            var paymentRequest = new CreateOutboundPaymentRequest
            {
                Payment = new PaymentRequest
                {
                    Message = request.Message,
                    TransactionText = request.TransactionText,
                    Amount = new PaymentAmountRequest
                    {
                        Value = request.Amount
                    },
                    Destination = new PaymentDestinationRequest(),
                    PaymentMethod = request.PaymentMethod,
                },
            };

            paymentRequest.Payment.Destination.RecipientFullname = request.RecipientFullname;

            if (!string.IsNullOrWhiteSpace(request.Iban))
            {
                paymentRequest.Payment.Destination.IBan = request.Iban;
            }
            else if(!string.IsNullOrWhiteSpace(request.BbanAccountNumber))
            {
                paymentRequest.Payment.Destination.BBan = new PaymentBBanRequest
                {
                    BankCode = request.BbanBankCode,
                    AccountNumber = request.BbanAccountNumber
                };
            }
            else
            {
                paymentRequest.Payment.Destination.InpaymentForm = new PaymentInpaymentFormRequest
                {
                    Type = request.InpaymentFormType,
                    CreditorNumber = request.InpaymentFormCreditorNumber
                };
            }

            return await CallApi<CreatePaymentResponseV2>($"v2/accounts/{request.SourceAccountId}/payments/outbound",
                                                        paymentRequest,
                                                        HttpMethod.Post,
                                                        user.AiiaTokenType,
                                                        user.AiiaAccessToken);
        }

        public async Task<CreatePaymentAuthorizationResponse> CreatePaymentAuthorization(ClaimsPrincipal principal, CreatePaymentAuthorizationRequestViewModel request)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            var paymentAuthorizationRequest = new CreatePaymentAuthorizationRequest()
            {
                Culture = request.Culture,
                PaymentIds = request.PaymentIds.ToArray(),
                RedirectUrl = GetPaymentAuthorizationRedirectUrl(),
            };

            return await CallApi<CreatePaymentAuthorizationResponse>($"v2/accounts/{request.SourceAccountId}/payments/outbound/authorizations",
                paymentAuthorizationRequest,
                HttpMethod.Post,
                user.AiiaTokenType,
                user.AiiaAccessToken);
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
                                    redirect_uri = GetRedirectUrl()
                };

                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                              {
                                  Content = new StringContent(JsonConvert.SerializeObject(tokenBody),
                                                              Encoding.UTF8,
                                                              "application/json")
                              };

                var response = await httpClient.SendAsync(request);
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

        public async Task<TransactionsResponse> GetAccountTransactions(ClaimsPrincipal principal,
                                                                       string accountId,
                                                                       TransactionQueryRequestViewModel queryRequest = null)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
                return null;

            return await HttpPost<TransactionsResponse>(
                                                        $"/v1/accounts/{accountId}/transactions/query?includeDeleted={queryRequest?.IncludeDeleted.ToString() ?? "false"}",
                                                        new
                                                        {
                                                            Interval = new Interval(SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(900)),
                                                                                    SystemClock.Instance.GetCurrentInstant()),
                                                            queryRequest?.PagingToken,
                                                            PageSize = 20,
                                                            Patterns = queryRequest?.Filters.Select(MapQueryPartToAiiaQueryPart).ToList(),
                                                            queryRequest?.AmountValueBetween,
                                                            queryRequest?.BalanceValueBetween
                                                        },
                                                        user.AiiaTokenType,
                                                        user.AiiaAccessToken,
                                                        principal);
        }

        public Uri GetAuthUri(string email, bool oneTime = false)
        {
            var connectUrl =
                $"{_options.CurrentValue.Aiia.BaseApiUrl}/v1/oauth/connect" +
                $"?client_id={_options.CurrentValue.Aiia.ClientId}" +
                "&response_type=code" +
                $"&redirect_uri={GetRedirectUrl()}" +
                $"&flow={(oneTime ? "OneTimeUser" : "PersistentUser")}";

            return new Uri(connectUrl);
        }

        public async Task<InboundPayment> GetInboundPayment(ClaimsPrincipal principal,
                                                            string accountId,
                                                            string paymentId)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            return await CallApi<InboundPayment>($"v1/accounts/{accountId}/payments/{paymentId}/inbound",
                                                 null,
                                                 HttpMethod.Get,
                                                 user.AiiaTokenType,
                                                 user.AiiaAccessToken);
        }

        public async Task<OutboundPayment> GetOutboundPayment(ClaimsPrincipal principal,
                                                              string accountId,
                                                              string paymentId)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            return await CallApi<OutboundPayment>($"v1/accounts/{accountId}/payments/{paymentId}/outbound",
                                                  null,
                                                  HttpMethod.Get,
                                                  user.AiiaTokenType,
                                                  user.AiiaAccessToken);
        }

        public async Task<PaymentAuthorization> GetPaymentAuthorization(ClaimsPrincipal principal, string accountId, string authorizationId)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            return await CallApi<PaymentAuthorization>($"v2/accounts/{accountId}/payments/outbound/authorizations/{authorizationId}",
                null,
                HttpMethod.Get,
                user.AiiaTokenType,
                user.AiiaAccessToken);
        }

        public async Task<PaymentsResponse> GetPayments(ClaimsPrincipal principal)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            var request = new PaymentsQueryRequest
                          {
                              PageSize = 100,
                              PagingToken = null
                          };

            return await CallApi<PaymentsResponse>("v1/payments/query",
                                                   request,
                                                   HttpMethod.Post,
                                                   user.AiiaTokenType,
                                                   user.AiiaAccessToken);
        }

        public Task<ImmutableList<BankProvider>> GetProviders()
        {
            return HttpGet<ImmutableList<BankProvider>>("/v1/providers");
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
                await HttpGet<AccountsResponse>("/v1/accounts", user.AiiaTokenType, user.AiiaAccessToken, principal);
            return result?.Accounts.ToImmutableList();
        }

        public Task<InitiateDataUpdateResponse> InitiateDataUpdate(ClaimsPrincipal principal)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return null;
            }

            var redirectUrl = $"{GetBaseUrl()}/aiia/data/{currentUserId}/";
            var requestBody = new InitiateDataUpdateRequest { RedirectUrl = redirectUrl };

            return HttpPost<InitiateDataUpdateResponse>("v1/update",
                                                        requestBody,
                                                        user.AiiaTokenType,
                                                        user.AiiaAccessToken);
        }

        public async Task ProcessWebHookPayload(HttpRequest request)
        {
            var payloadString = await ReadRequestBody(request.Body);

            _logger.LogInformation($"Received webhook: {payloadString}");
            // `X-Aiia-Signature` is provided optionally if client has configured `WebhookSecret` and is used to verify that webhook was sent by Aiia
            var aiiaSignature = request.Headers["X-Aiia-Signature"];
            if (!VerifySignature(aiiaSignature, payloadString))
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

            var user = _dbContext.Users.FirstOrDefault(x => x.AiiaConsentId == consentId);
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
                                    redirect_uri = GetRedirectUrl()
                };

                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                              {
                                  Content = new StringContent(JsonConvert.SerializeObject(tokenBody),
                                                              Encoding.UTF8,
                                                              "application/json")
                              };

                var response = await httpClient.SendAsync(request);
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

        private async Task<T> CallApi<T>(string url,
                                         object body,
                                         HttpMethod method,
                                         string accessTokenType = null,
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
                                                                                                     new JsonSerializerSettings()
                                                                                                         .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)
                                                                                                         .WithIsoIntervalConverter()),
                                                                         Encoding.UTF8,
                                                                         "application/json")
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
                                 "Aiia request: {RequestUri} {StatusCode} {DurationMilliseconds}ms",
                                 result.RequestMessage.RequestUri,
                                 result.StatusCode,
                                 Math.Round(duration.TotalMilliseconds)
                                );

                if (!result.IsSuccessStatusCode)
                {
                    responseContent = await result.Content.ReadAsStringAsync();
                    throw new AiiaClientException(url, result.StatusCode, responseContent);
                }

                responseContent = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent,
                                                        new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)
                                                                                    .WithIsoIntervalConverter());
            }
            catch (AiiaClientException)
            {
                throw;
            }
            catch (JsonSerializationException e)
            {
                throw new AiiaClientException(url, method, responseContent, e);
            }
            catch (Exception e)
            {
                throw new AiiaClientException(url, method, result, e);
            }
        }

        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Authentication
        // TL;DR:
        // 1. Create string - `{your aiia client id}:{your aiia client secret}`
        // 2. Convert that string to byte array using `iso-8859-1` encoding
        // 3. Convert that byte array to base 64
        private string GenerateBasicAuthorizationHeaderValue()
        {
            var credentials = $"{_options.CurrentValue.Aiia.ClientId}:{_options.CurrentValue.Aiia.ClientSecret}";
            var credentialsByteData = Encoding.GetEncoding("iso-8859-1").GetBytes(credentials);
            var base64Credentials = Convert.ToBase64String(credentialsByteData);
            return $"{base64Credentials}";
        }

        // Generate HMAC hash of webhook payload using secret shared with Aiia
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

        // Gets the base url of current environment that sample app is running
        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;

            var host = request.Host.ToUriComponent();

            var pathBase = request.PathBase.ToUriComponent();

            return $"{request.Scheme}://{host}{pathBase}";
        }

        private string GetPaymentAuthorizationRedirectUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{request.PathBase}/v2/aiia/payment-authorizations/callback";
        }

        private string GetPaymentRedirectUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{request.PathBase}/aiia/payments/callback";
        }

        private string GetRedirectUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{request.PathBase}/aiia/callback";
        }

        private async Task<Transaction> GetTransaction(ClaimsPrincipal principal,
                                                       string accountId,
                                                       string transactionId)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
                return null;

            return await HttpGet<Transaction>($"/v1/accounts/{accountId}/transactions/{transactionId}",
                                              user.AiiaTokenType,
                                              user.AiiaAccessToken,
                                              principal);
        }

        private async Task<T> HttpGet<T>(string url,
                                         string accessTokenType = null,
                                         string accessToken = null,
                                         ClaimsPrincipal principal = null,
                                         bool isRetry = false)
        {
            try
            {
                return await CallApi<T>(url, null, HttpMethod.Get, accessTokenType, accessToken);
            }
            catch (AiiaClientException e) when (e.StatusCode == HttpStatusCode.Unauthorized && accessToken.IsSet() &&
                                                !isRetry)
            {
                var updatedTokens = await RefreshAccessTokenAndSaveToUser(principal);
                return await HttpGet<T>(url, updatedTokens.TokenType, updatedTokens.AccessToken, principal, true);
            }
        }

        private async Task<T> HttpPost<T>(string url,
                                          object body,
                                          string accessTokenType = null,
                                          string accessToken = null,
                                          ClaimsPrincipal principal = null,
                                          bool isRetry = false)
        {
            try
            {
                return await CallApi<T>(url, body, HttpMethod.Post, accessTokenType, accessToken);
            }
            catch (AiiaClientException e) when (e.StatusCode == HttpStatusCode.Unauthorized && accessToken.IsSet() &&
                                                !isRetry)
            {
                var updatedTokens = await RefreshAccessTokenAndSaveToUser(principal);
                return await HttpPost<T>(url,
                                         body,
                                         updatedTokens.TokenType,
                                         updatedTokens.AccessToken,
                                         principal,
                                         true);
            }
        }

        private AiiaQueryPart MapQueryPartToAiiaQueryPart(QueryPart filter)
        {
            return new AiiaQueryPart
                   {
                       IncludedQueryProperties = new List<string> { filter.Property },
                       Pattern = filter.Value,
                       Operator = filter.Operator,
                   };
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

        private async Task<CodeExchangeResponse> RefreshAccessTokenAndSaveToUser(ClaimsPrincipal principal)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null)
            {
                return null;
            }

            var result = await RefreshAccessToken(user.AiiaRefreshToken);
            user.AiiaAccessToken = result.AccessToken;
            user.AiiaRefreshToken = result.RefreshToken;
            user.AiiaTokenType = result.TokenType;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return result;
        }

        // Aiia calculates same HMAC hash using the secret only known by the client and Aiia
        // If HMAC hashes doesn't mach, it means that the webhook was not sent by Aiia
        private bool VerifySignature(string aiiaSignature, string payload)
        {
            if (string.IsNullOrWhiteSpace(aiiaSignature))
                return true;

            if (string.IsNullOrWhiteSpace(_options.CurrentValue.Aiia.WebHookSecret))
                return true;

            var generatedSignature = GenerateHmacSignature(payload, _options.CurrentValue.Aiia.WebHookSecret);

            if (generatedSignature != aiiaSignature)
            {
                _logger.LogWarning(
                                   $"Webhook signatures didn't match. Received:\n{aiiaSignature}\nGenerated: {generatedSignature}");
                return false;
            }

            return true;
        }
    }
}
