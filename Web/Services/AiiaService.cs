using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Aiia.Sample.AiiaClient.Models;
using Aiia.Sample.AiiaClient.Models.V2;
using Aiia.Sample.Data;
using Aiia.Sample.Exceptions;
using Aiia.Sample.Extensions;
using Aiia.Sample.Models;
using Aiia.Sample.Models.V2;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Aiia.Sample.AiiaClient;

public class AiiaService : IAiiaService
{
    private readonly AiiaHttpClient _aiiaHttpClient;
    private readonly AiiaApi _api;
    public readonly ApplicationDbContext _dbContext;
    public readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsMonitor<SiteOptions> _options;

    public AiiaService(IOptionsMonitor<SiteOptions> options,
        ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        AiiaApi api)
    {
        _options = options;
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _api = api;
    }

    public AiiaClientSecret ClientSecret => new()
    {
        ClientId = _options.CurrentValue.Aiia.ClientId,
        Secret = _options.CurrentValue.Aiia.ClientSecret
    };


    public async Task<bool?> AllAccountsSelected(ClaimsPrincipal principal)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);
        
        try
        {
            var response = await _api.AllAccountsSelected(user.GetAiiaAccessTokens(), user.AiiaConsentId);
            return response.AllAccountsSelected;
        }
        catch (AiiaClientException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            // we are not allowed to check if all the accounts were selected for this type of user.
            // convert it into a null.
            return null;
        }

    }


    public async Task ExchangeCodeForAccessToken(ClaimsPrincipal principal, string code, string consentId)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        var tokenResponse = await _api.AuthenticationCodeExchange(ClientSecret, code, GetRedirectUrl());
        
        user.AiiaAccessToken = tokenResponse.AccessToken;
        user.AiiaTokenType = tokenResponse.TokenType;
        user.AiiaRefreshToken = tokenResponse.RefreshToken;
        user.AiiaAccessTokenExpires = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
        user.AiiaConsentId = consentId;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }



    public async Task<CreatePaymentResponse> CreateInboundPayment(ClaimsPrincipal principal,
        CreatePaymentRequestViewModel request)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);

        var paymentRequest = new CreateInboundPaymentRequest
        {
            Culture = request.Culture,
            RedirectUrl = GetInboundPaymentRedirectUrl(),
            IssuePayerToken = request.IssuePayerToken,
            PayerToken = request.PayerToken,
            ProviderId = request.ProviderId,
            Payment = new InboundPaymentRequest
            {
                Amount = new PaymentAmountRequest
                {
                    Value = request.Amount
                },
                OrderId = request.OrderId
            }
        };

        return await _api.CreateInboundPaymentV1(user.GetAiiaAccessTokens(), request.SourceAccountId, paymentRequest);
    }

    public async Task<CreatePaymentResponseV2> CreateOutboundPaymentV2(ClaimsPrincipal principal,
        CreatePaymentRequestViewModelV2 request)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);

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

        return await _api.CreateOutboundPaymentV2(user.GetAiiaAccessTokens(), request.SourceAccountId, paymentRequest);
    }

    public async Task<CreatePaymentAuthorizationResponse> CreatePaymentAuthorization(ClaimsPrincipal principal,
        CreatePaymentAuthorizationRequestViewModel request)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);

        var paymentAuthorizationRequest = new CreatePaymentAuthorizationRequest
        {
            Culture = request.Culture,
            PaymentIds = request.PaymentIds.ToArray(),
            RedirectUrl = GetOutboundPaymentAuthorizationRedirectUrl()
        };

        return await _api.CreatePaymentAuthorization(user.GetAiiaAccessTokens(), request.SourceAccountId,
            paymentAuthorizationRequest);
    }


    public async Task<TransactionsResponse> GetAccountTransactions(ClaimsPrincipal principal,
        string accountId,
        TransactionQueryRequestViewModel queryRequest = null)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);

        var request = new TransactionQueryRequest
        {
            PagingToken = queryRequest?.PagingToken,
            PageSize = 20,
            AmountValueBetween = queryRequest?.AmountValueBetween,
            BalanceValueBetween = queryRequest?.BalanceValueBetween
        };

        return await _api.GetAccountTransactions(user.GetAiiaAccessTokens(), accountId, queryRequest?.IncludeDeleted ?? false,
            request);
    }

    public async Task<InboundPayment> GetInboundPayment(ClaimsPrincipal principal,
        string accountId,
        string paymentId)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);

        var payment = await _api.GetInboundPayment(user.GetAiiaAccessTokens(), accountId, paymentId);

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
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);
        
        return await _api.GetOutboundPayment(user.GetAiiaAccessTokens(), accountId, paymentId);
    }

    public async Task<OutboundPaymentV2Response> GetOutboundPaymentV2(ClaimsPrincipal principal,
        string accountId,
        string paymentId)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);
        
        return await _api.GetOutboundPaymentV2(user.GetAiiaAccessTokens(), accountId, paymentId);
    }

    public async Task<PaymentAuthorization> GetPaymentAuthorization(ClaimsPrincipal principal, string accountId,
        string authorizationId)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);
        
        return await _api.GetPaymentAuthorization(user.GetAiiaAccessTokens(), accountId, authorizationId);
    }

    public async Task<PaymentsResponse> GetPayments(ClaimsPrincipal principal)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);

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

    public async Task<IImmutableList<Account>> GetAccounts(ClaimsPrincipal principal)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);

        var result = await _api.GetAccounts(user.GetAiiaAccessTokens());

        return result?.Accounts.ToImmutableList();
    }

    public async Task<InitiateDataUpdateResponse> InitiateDataUpdate(ClaimsPrincipal principal)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);

        var redirectUrl = $"{GetBaseUrl()}/aiia/data/{user.Id}/";
        var requestBody = new InitiateDataUpdateRequest { RedirectUrl = redirectUrl };

        return await _api.InitiateDataUpdate(user.GetAiiaAccessTokens(), requestBody);
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

    private async Task RefreshAccessTokenIfNeeded(ApplicationUser user)
    {
        if (user.AiiaAccessTokenExpires > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            // less than 1 minute remaining, so we just continue using the existing access token
            return;
        }

        await RefreshAccessToken(user);
    }

    private async Task RefreshAccessToken(ApplicationUser user)
    {
        // refresh token
        var startTime = DateTimeOffset.Now;
        try
        {
            var result = await _api.AuthenticationRefreshToken(ClientSecret, user.AiiaRefreshToken, GetRedirectUrl());
            
            // update the database (and the passed object by reference)
            user.AiiaAccessToken = result.AccessToken;
            user.AiiaRefreshToken = result.RefreshToken;
            user.AiiaTokenType = result.TokenType;
            user.AiiaAccessTokenExpires = startTime.AddSeconds(result.ExpiresIn);
        
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
        catch(AiiaClientException ex)
        {
            if (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // The refresh token expired too.
                return;
            }

            throw;
        }
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

    private string GetOutboundPaymentAuthorizationRedirectUrl()
    {
        var request = _httpContextAccessor.HttpContext.Request;
        return $"{request.Scheme}://{request.Host}{request.PathBase}/aiia/payments/outbound/payment-authorizations/callback";
    }

    private string GetInboundPaymentRedirectUrl()
    {
        var request = _httpContextAccessor.HttpContext.Request;
        return $"{request.Scheme}://{request.Host}{request.PathBase}/aiia/payments/inbound/callback";
    }

    private async Task<Transaction> GetTransaction(ClaimsPrincipal principal,
        string accountId,
        string transactionId)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);

        return await _api.GetTransaction(user.GetAiiaAccessTokens(), accountId, transactionId);
    }

    public async Task<PaymentReconciliationV1Response> GetPaymentReconciliationV1(ClaimsPrincipal principal,
        string accountId, string paymentId)
    {
        var user = await principal.GetCurrentUser(_dbContext);
        await RefreshAccessTokenIfNeeded(user);
        
        return await _api.GetPaymentReconciliationV1(user.GetAiiaAccessTokens(), accountId, paymentId);
    }

    public string GetRedirectUrl()
    {
        var request = _httpContextAccessor.HttpContext.Request;
        return $"{request.Scheme}://{request.Host}{request.PathBase}/aiia/callback";
    }
}