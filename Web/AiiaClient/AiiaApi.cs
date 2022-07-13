using System.Collections.Immutable;
using System.Threading.Tasks;
using Aiia.Sample.AiiaClient.Models;
using Aiia.Sample.AiiaClient.Models.V2;

namespace Aiia.Sample.AiiaClient;

public class AiiaApi
{
    private readonly IAiiaHttpClient _aiiaHttpClient;

    public AiiaApi(IAiiaHttpClient aiiaHttpClient)
    {
        _aiiaHttpClient = aiiaHttpClient;
    }

    public async Task<AllAccountSelectedResponse> AllAccountsSelected(AiiaAccessToken accessToken, string consentId)
    {
        return await _aiiaHttpClient.HttpGet<AllAccountSelectedResponse>(
            $"v1/consent/{consentId}/all-accounts-selected",
            accessToken);
    }

    public async Task<CodeExchangeResponse> AuthenticationCodeExchange(AiiaClientSecret clientSecret, string code,
        string redirectUri)
    {
        CodeExchangeRequest tokenBody = new()
        {
            grant_type = "authorization_code",
            code = code,
            scope = "read",
            redirect_uri = redirectUri
        };

        return await _aiiaHttpClient.HttpPost<CodeExchangeRequest, CodeExchangeResponse>("v1/oauth/token", tokenBody,
            clientSecret);
    }

    public async Task<CodeExchangeResponse> AuthenticationRefreshToken(AiiaClientSecret clientSecret,
        string refreshToken, string redirectUri)
    {
        CodeExchangeRequest tokenBody = new()
        {
            grant_type = "refresh_token",
            refresh_token = refreshToken,
            scope = "read",
            redirect_uri = redirectUri
        };

        return await _aiiaHttpClient.HttpPost<CodeExchangeRequest, CodeExchangeResponse>("v1/oauth/token", tokenBody,
            clientSecret);
    }

    public async Task<CreatePaymentResponse> CreateInboundPaymentV1(AiiaAccessToken accessToken,
        string targetAccountId,
        CreateInboundPaymentRequest paymentRequest)
    {
        return await _aiiaHttpClient.HttpPost<CreateInboundPaymentRequest, CreatePaymentResponse>(
            $"v1/accounts/{targetAccountId}/payments/inbound",
            paymentRequest, accessToken);
    }

    public async Task<CreatePaymentResponseV2> CreateOutboundPaymentV2(AiiaAccessToken accessToken, string accountId,
        CreateOutboundPaymentRequestV2 paymentRequest)
    {
        return await _aiiaHttpClient.HttpPost<CreateOutboundPaymentRequestV2, CreatePaymentResponseV2>(
            $"v2/accounts/{accountId}/payments",
            paymentRequest, accessToken);
    }

    public async Task<CreatePaymentAuthorizationResponse> CreatePaymentAuthorization(AiiaAccessToken accessToken,
        string accountId, CreatePaymentAuthorizationRequest authorizationRequest)
    {
        return await _aiiaHttpClient.HttpPost<CreatePaymentAuthorizationRequest, CreatePaymentAuthorizationResponse>(
            $"v2/accounts/{accountId}/payment-authorizations", authorizationRequest, accessToken);
    }

    public async Task<TransactionsResponse> GetAccountTransactions(AiiaAccessToken accessToken, string accountId,
        bool includeDeleted, TransactionQueryRequest request)
    {
        return await _aiiaHttpClient.HttpPost<TransactionQueryRequest, TransactionsResponse>(
            $"/v1/accounts/{accountId}/transactions/query?includeDeleted={includeDeleted.ToString()}",
            request, accessToken);
    }

    public async Task<InboundPayment> GetInboundPayment(AiiaAccessToken accessToken, string accountId, string paymentId)
    {
        return await _aiiaHttpClient.HttpGet<InboundPayment>($"v1/accounts/{accountId}/payments/inbound/{paymentId}",
            accessToken);
    }

    public async Task<PayerTokenModel> GetInboundPaymentPayerToken(AiiaAccessToken accessToken, string accountId,
        string paymentId)
    {
        return await _aiiaHttpClient.HttpPost<object, PayerTokenModel>(
            $"v1/accounts/{accountId}/payments/inbound/{paymentId}/payer-token", null, accessToken);
    }

    public async Task<OutboundPayment> GetOutboundPayment(AiiaAccessToken accessToken, string accountId,
        string paymentId)
    {
        return await _aiiaHttpClient.HttpGet<OutboundPayment>($"v1/accounts/{accountId}/payments/{paymentId}/outbound",
            accessToken);
    }
    
    public async Task<OutboundPaymentV2Response> GetOutboundPaymentV2(AiiaAccessToken accessToken, string accountId, string paymentId)
    {
        return await _aiiaHttpClient.HttpGet<OutboundPaymentV2Response>($"v2/accounts/{accountId}/payments/{paymentId}",
            accessToken);
    }

    public async Task<PaymentAuthorization> GetPaymentAuthorization(AiiaAccessToken accessToken, string accountId,
        string authorizationId)
    {
        return await _aiiaHttpClient.HttpGet<PaymentAuthorization>(
            $"v2/accounts/{accountId}/payment-authorizations/{authorizationId}",
            accessToken);
    }

    public async Task<PaymentsResponse> QueryPayments(AiiaAccessToken accessToken, PaymentsQueryRequest request)
    {
        return await _aiiaHttpClient.HttpPost<PaymentsQueryRequest, PaymentsResponse>("v1/payments/query",
            request, accessToken);
    }

    public Task<ImmutableList<BankProvider>> GetProviders()
    {
        return _aiiaHttpClient.HttpGet<ImmutableList<BankProvider>>("/v1/providers");
    }

    public async Task<AccountsResponse> GetAccounts(AiiaAccessToken accessToken)
    {
        return await _aiiaHttpClient.HttpGet<AccountsResponse>("/v1/accounts", accessToken);
    }

    public async Task<InitiateDataUpdateResponse> InitiateDataUpdate(AiiaAccessToken accessToken,
        InitiateDataUpdateRequest requestBody)
    {
        return await _aiiaHttpClient.HttpPost<InitiateDataUpdateRequest, InitiateDataUpdateResponse>("v1/update",
            requestBody,
            accessToken);
    }

    public async Task<Transaction> GetTransaction(AiiaAccessToken accessToken, string accountId, string transactionId)
    {
        return await _aiiaHttpClient.HttpGet<Transaction>($"/v1/accounts/{accountId}/transactions/{transactionId}",
            accessToken);
    }
    public async Task<PaymentReconciliationV1Response> GetPaymentReconciliationV1(AiiaAccessToken accessToken, string accountId, string paymentId)
    {
        return await _aiiaHttpClient.HttpGet<PaymentReconciliationV1Response>($"/v1/accounts/{accountId}/payments/inbound/{paymentId}/reconciliation",
            accessToken);
    }

}