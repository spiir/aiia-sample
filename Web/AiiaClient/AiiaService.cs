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

namespace Aiia.Sample.Services
{
    public class AiiaService : IAiiaService
    {
        public readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly AiiaApi _api;
        public readonly IHttpContextAccessor _httpContextAccessor;
        public readonly ILogger<AiiaService> _logger;
        private readonly AiiaHttpClient _aiiaHttpClient;
        private readonly IOptionsMonitor<SiteOptions> _options;

        public AiiaService(IOptionsMonitor<SiteOptions> options,
            ILogger<AiiaService> logger,
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService, 
            AiiaApi api)
        {
            _options = options;
            _logger = logger;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _api = api;
        }

        private ApplicationUser GetCurrentUser(ClaimsPrincipal principal)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null) throw new UserNotFoundException();
            return user;
        }

        public AiiaClientSecrets ClientSecret => new()
        {
            ClientId = _options.CurrentValue.Aiia.ClientId,
            Secret = _options.CurrentValue.Aiia.ClientSecret
        };

        
        public async Task<bool?> AllAccountsSelected(ClaimsPrincipal principal)
        {
            var user = GetCurrentUser(principal);
            var response = await _api.AllAccountsSelected(user.GetAiiaAccessTokens(), user.AiiaConsentId);
            return response.AllAccountsSelected;
        }


        public async Task<CodeExchangeResponse> ExchangeCodeForAccessToken(string code)
        {
            return await _api.AuthenticationCodeExchange(ClientSecret, code, GetRedirectUrl());
        }
        private async Task<CodeExchangeResponse> RefreshAccessTokenAndSaveToUser(ClaimsPrincipal principal)
        {
            var user = GetCurrentUser(principal);

            var result = await RefreshAccessToken(user.AiiaRefreshToken);
            user.AiiaAccessToken = result.AccessToken;
            user.AiiaRefreshToken = result.RefreshToken;
            user.AiiaTokenType = result.TokenType;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return result;
        }

        public async Task<CodeExchangeResponse> RefreshAccessToken(string refreshToken)
        {
            return await _api.AuthenticationRefreshToken(ClientSecret, refreshToken, GetRedirectUrl());
        }


        public async Task<CreatePaymentResponse> CreateInboundPayment(ClaimsPrincipal principal,
            CreatePaymentRequestViewModel request)
        {
            var user = GetCurrentUser(principal);
            
            var paymentRequest = new CreateInboundPaymentRequest
            {
                Culture = request.Culture,
                RedirectUrl = GetPaymentRedirectUrl(),
                IssuePayerToken = request.IssuePayerToken,
                PayerToken = request.PayerToken,
                ProviderId = request.ProviderId,
                Payment = new InboundPaymentRequest
                {
                    Amount = new PaymentAmountRequest
                    {
                        Value = request.Amount
                    }
                }
            };

            return await _api.CreateInboundPaymentV1(user.GetAiiaAccessTokens(), request.SourceAccountId, paymentRequest);
        }

        public async Task<CreatePaymentResponse> CreateOutboundPayment(ClaimsPrincipal principal,
            CreatePaymentRequestViewModel request)
        {
            var user = GetCurrentUser(principal);

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
                    PaymentMethod = request.PaymentMethod
                }
            };

            paymentRequest.Payment.Destination.RecipientFullname = request.RecipientFullname;

            if (!string.IsNullOrWhiteSpace(request.Iban))
                paymentRequest.Payment.Destination.IBan = request.Iban;
            
            else if (!string.IsNullOrWhiteSpace(request.BbanAccountNumber))
                paymentRequest.Payment.Destination.BBan = new PaymentBBanRequest
                {
                    BankCode = request.BbanBankCode,
                    AccountNumber = request.BbanAccountNumber
                };
            else
                paymentRequest.Payment.Destination.InpaymentForm = new PaymentInpaymentFormRequest
                {
                    Type = request.InpaymentFormType,
                    CreditorNumber = request.InpaymentFormCreditorNumber
                };

            if (!string.IsNullOrEmpty(request.Ocr))
                paymentRequest.Payment.Identifiers = new PaymentIdentifiersRequest { Ocr = request.Ocr };

            if (!string.IsNullOrEmpty(request.AddressStreet))
                paymentRequest.Payment.Destination.Address = new PaymentAddressRequest
                {
                    Street = request.AddressStreet,
                    BuildingNumber = request.AddressBuildingNumber,
                    PostalCode = request.AddressPostalCode,
                    City = request.AddressCity,
                    Country = request.AddressCountry
                };

            return await _api.CreateOutboundPaymentV1(user.GetAiiaAccessTokens(), request.SourceAccountId, paymentRequest);
        }

        public async Task<CreatePaymentResponseV2> CreatePaymentV2(ClaimsPrincipal principal,
            CreatePaymentRequestViewModelV2 request)
        {
            var user = GetCurrentUser(principal);

            var paymentRequest = new CreateOutboundPaymentRequestV2
            {
                Payment = new PaymentRequestV2
                {
                    Message = request.Message,
                    TransactionText = request.TransactionText,
                    Amount = new PaymentAmountRequestV2
                    {
                        Value = request.Amount,
                        Currency = request.Currency
                    },
                    Destination = new PaymentDestinationRequestV2(),
                    PaymentMethod = request.PaymentMethod
                }
            };

            paymentRequest.Payment.Destination.Name = request.RecipientFullname;

            if (!string.IsNullOrWhiteSpace(request.Iban))
                paymentRequest.Payment.Destination.IBan = new PaymentIbanRequestV2 { IbanNumber = request.Iban };
            else if (!string.IsNullOrWhiteSpace(request.BbanAccountNumber))
                paymentRequest.Payment.Destination.BBan = new PaymentBBanRequest
                {
                    BankCode = request.BbanBankCode,
                    AccountNumber = request.BbanAccountNumber
                };
            else
                paymentRequest.Payment.Destination.InpaymentForm = new PaymentInpaymentFormRequest
                {
                    Type = request.InpaymentFormType,
                    CreditorNumber = request.InpaymentFormCreditorNumber
                };

            if (!string.IsNullOrEmpty(request.Ocr))
                paymentRequest.Payment.Identifiers = new PaymentIdentifiersRequest { Ocr = request.Ocr };

            if (!string.IsNullOrEmpty(request.AddressStreet))
                paymentRequest.Payment.Destination.Address = new PaymentAddressRequest
                {
                    Street = request.AddressStreet,
                    BuildingNumber = request.AddressBuildingNumber,
                    PostalCode = request.AddressPostalCode,
                    City = request.AddressCity,
                    Country = request.AddressCountry
                };

            return await _api.CreatePaymentV2(user.GetAiiaAccessTokens(), request.SourceAccountId, paymentRequest);
        }

        public async Task<CreatePaymentAuthorizationResponse> CreatePaymentAuthorization(ClaimsPrincipal principal,
            CreatePaymentAuthorizationRequestViewModel request)
        {
            var user = GetCurrentUser(principal);

            var paymentAuthorizationRequest = new CreatePaymentAuthorizationRequest
            {
                Culture = request.Culture,
                PaymentIds = request.PaymentIds.ToArray(),
                RedirectUrl = GetPaymentAuthorizationRedirectUrl()
            };

            return await _api.CreatePaymentAuthorization(user.GetAiiaAccessTokens(), request.SourceAccountId,
                paymentAuthorizationRequest);
        }


        public async Task<TransactionsResponse> GetAccountTransactions(ClaimsPrincipal principal,
            string accountId,
            TransactionQueryRequestViewModel queryRequest = null)
        {
            var user = GetCurrentUser(principal);

            TransactionQueryRequest request = new TransactionQueryRequest()
            {
                Interval = "", // TODO: new Interval(SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(900)), SystemClock.Instance.GetCurrentInstant()),
                PagingToken = queryRequest?.PagingToken,
                PageSize = 20,
                Patterns = queryRequest?.Filters.Select(MapQueryPartToAiiaQueryPart).ToList(),
                AmountValueBetween = queryRequest?.AmountValueBetween,
                BalanceValueBetween = queryRequest?.BalanceValueBetween

            };

            return await _api.GetAccountTransactions(user.GetAiiaAccessTokens(), accountId,queryRequest.IncludeDeleted, request);
        }

        public async Task<InboundPayment> GetInboundPayment(ClaimsPrincipal principal,
            string accountId,
            string paymentId)
        {
            var user = GetCurrentUser(principal);

            InboundPayment payment = await _api.GetInboundPayment(user.GetAiiaAccessTokens(), accountId, paymentId);
            
            try
            {
                var payerToken =
                    await _api.GetInboundPaymentPayerToken(user.GetAiiaAccessTokens(), accountId, paymentId);

                payment.PayerToken = payerToken;
            }
            catch (AiiaClientException)
            {
                // Ignore if there is no payer token available
            }

            return payment;
        }

        public async Task<OutboundPayment> GetOutboundPayment(ClaimsPrincipal principal,
            string accountId,
            string paymentId)
        {
            var user = GetCurrentUser(principal);
            return await _api.GetOutboundPayment(user.GetAiiaAccessTokens(), accountId, paymentId);
        }

        public async Task<PaymentAuthorization> GetPaymentAuthorization(ClaimsPrincipal principal, string accountId,
            string authorizationId)
        {
            var user = GetCurrentUser(principal);
            return await _api.GetPaymentAuthorization(user.GetAiiaAccessTokens(), accountId, authorizationId);

        }

        public async Task<PaymentsResponse> GetPayments(ClaimsPrincipal principal)
        {
            var user = GetCurrentUser(principal);

            var request = new PaymentsQueryRequest
            {
                PageSize = 100,
                PagingToken = null
            };

            return await _api.QueryPayments(user.GetAiiaAccessTokens(), request);
        }

        public Task<ImmutableList<BankProvider>> GetProviders()
        {
            return _api.GetProviders();
        }

        public async Task<IImmutableList<Account>> GetUserAccounts(ClaimsPrincipal principal)
        {
            var user = GetCurrentUser(principal);

            var result = await _api.GetUserAccounts(user.GetAiiaAccessTokens());
            
            return result?.Accounts.ToImmutableList();
        }

        public Task<InitiateDataUpdateResponse> InitiateDataUpdate(ClaimsPrincipal principal)
        {
            var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == currentUserId);
            if (user == null) return null;

            var redirectUrl = $"{GetBaseUrl()}/aiia/data/{currentUserId}/";
            var requestBody = new InitiateDataUpdateRequest { RedirectUrl = redirectUrl };

            return _api.InitiateDataUpdate(user.GetAiiaAccessTokens(), requestBody);
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


        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Authentication
        // TL;DR:
        // 1. Create string - `{your aiia client id}:{your aiia client secret}`
        // 2. Convert that string to byte array using `iso-8859-1` encoding
        // 3. Convert that byte array to base 64

        // Generate HMAC hash of webhook payload using secret shared with Aiia

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

        private async Task<Transaction> GetTransaction(ClaimsPrincipal principal,
            string accountId,
            string transactionId)
        {
            var user = GetCurrentUser(principal);

            return await _api.GetTransaction(user.GetAiiaAccessTokens(), accountId, transactionId);
        }

        private AiiaQueryPart MapQueryPartToAiiaQueryPart(QueryPart filter)
        {
            return new AiiaQueryPart
            {
                IncludedQueryProperties = new List<string> { filter.Property },
                Pattern = filter.Value,
                Operator = filter.Operator
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

        // Aiia calculates same HMAC hash using the secret only known by the client and Aiia
        // If HMAC hashes doesn't mach, it means that the webhook was not sent by Aiia
       
        public bool VerifySignature(string aiiaSignature, string payload)
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

        
        public Uri GetAuthUri(string email)
        {
            var connectUrl =
                $"{_options.CurrentValue.Aiia.BaseApiUrl}/v1/oauth/connect" +
                $"?client_id={_options.CurrentValue.Aiia.ClientId}" +
                "&response_type=code" +
                $"&redirect_uri={GetRedirectUrl()}";

            return new Uri(connectUrl);
        }

        public string GetRedirectUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{request.PathBase}/aiia/callback";
        }

    }
}