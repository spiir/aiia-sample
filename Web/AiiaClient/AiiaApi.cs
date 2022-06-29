using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading.Tasks;
using Aiia.Sample.Models.Aiia;

namespace Aiia.Sample.Services
{
    public partial class AiiaApi
    {
        private IAiiaHttpClient _aiiaHttpClient;

        public async Task<AllAccountSelectedResponse> AllAccountsSelected(AiiaAccessTokens accessTokens, string consentId)
        {
            return await _aiiaHttpClient.HttpGet<AllAccountSelectedResponse>(
                $"v1/consent/{consentId}/all-accounts-selected",
                accessTokens);
        }

        public async Task<CodeExchangeResponse> AuthenticationCodeExchange(AiiaClientSecrets clientSecrets, string code, string redirectUri)
        {
            CodeExchangeRequest tokenBody = new()
            {
                grant_type = "authorization_code",
                code = code,
                scope = "read",
                redirect_uri = redirectUri
            };

            return await _aiiaHttpClient.HttpPost<CodeExchangeRequest, CodeExchangeResponse>("v1/oauth/token", tokenBody, clientSecrets);

        }
        public async Task<CodeExchangeResponse> AuthenticationRefreshToken(AiiaClientSecrets clientSecrets, string refreshToken, string redirectUri)
        {
            CodeExchangeRequest tokenBody = new()
            {
                grant_type = "refresh_token",
                refresh_token = refreshToken,
                scope = "read",
                redirect_uri = redirectUri
            };

            return await _aiiaHttpClient.HttpPost<CodeExchangeRequest, CodeExchangeResponse>("v1/oauth/token", tokenBody, clientSecrets);

        }

        public async Task<CreatePaymentResponse> CreateInboundPaymentV1(AiiaAccessTokens accessTokens,
            string targetAccountId,
            CreateInboundPaymentRequest paymentRequest)
        {
            return await _aiiaHttpClient.HttpPost<CreateInboundPaymentRequest, CreatePaymentResponse>(
                $"v1/accounts/{targetAccountId}/payments/inbound",
                paymentRequest,accessTokens);
        }

        public async Task<CreatePaymentResponse> CreateOutboundPaymentV1(AiiaAccessTokens accessTokens, string sourceAccountId, CreateOutboundPaymentRequest paymentRequest)
        {
            return await _aiiaHttpClient.HttpPost<CreateOutboundPaymentRequest, CreatePaymentResponse>(
                $"v1/accounts/{sourceAccountId}/payments/outbound",
                paymentRequest,accessTokens);
        }
        
        public async Task<CreatePaymentResponseV2> CreatePaymentV2(AiiaAccessTokens accessTokens, string accountId, CreateOutboundPaymentRequestV2 paymentRequest)
        {
            
            return await _aiiaHttpClient.HttpPost<CreateOutboundPaymentRequestV2, CreatePaymentResponseV2>(
                $"v2/accounts/{accountId}/payments",
                paymentRequest,accessTokens);
        }

        public async Task<CreatePaymentAuthorizationResponse> CreatePaymentAuthorization(AiiaAccessTokens accessTokens, string accountId, CreatePaymentAuthorizationRequest authorizationRequest)
        {
            return await _aiiaHttpClient.HttpPost<CreatePaymentAuthorizationRequest, CreatePaymentAuthorizationResponse>(
                $"v2/accounts/{accountId}/payment-authorizations",authorizationRequest, accessTokens);
        }

        public async Task<TransactionsResponse> GetAccountTransactions(AiiaAccessTokens accessTokens, string accountId, bool includeDeleted, TransactionQueryRequest request)
        {
            return await _aiiaHttpClient.HttpPost<TransactionQueryRequest, TransactionsResponse>(
                $"/v1/accounts/{accountId}/transactions/query?includeDeleted={includeDeleted.ToString()}",
                request,accessTokens);
        }

        public async Task< InboundPayment> GetInboundPayment(AiiaAccessTokens accessTokens, string accountId, string paymentId)
        {
            return await _aiiaHttpClient.HttpGet<InboundPayment>($"v1/accounts/{accountId}/payments/inbound/{paymentId}",accessTokens);
        }

        public async Task<PayerTokenModel> GetInboundPaymentPayerToken(AiiaAccessTokens accessTokens, string accountId,
            string paymentId)
        {
            return await _aiiaHttpClient.HttpPost<object, PayerTokenModel>(
                $"v1/accounts/{accountId}/payments/inbound/{paymentId}/payer-token", new object(), accessTokens);

        }

        public async Task<OutboundPayment> GetOutboundPayment(AiiaAccessTokens accessTokens, string accountId, string paymentId)
        {
            return await _aiiaHttpClient.HttpGet<OutboundPayment>($"v1/accounts/{accountId}/payments/{paymentId}/outbound",
                accessTokens);
        }

        public async Task<PaymentAuthorization> GetPaymentAuthorization(AiiaAccessTokens accessTokens, string accountId, string authorizationId)
        {
            
            return await _aiiaHttpClient.HttpGet<PaymentAuthorization>(
                $"v2/accounts/{accountId}/payment-authorizations/{authorizationId}",
                accessTokens);
        }

        public async Task<PaymentsResponse> QueryPayments(AiiaAccessTokens accessTokens, PaymentsQueryRequest request)
        {
            return await _aiiaHttpClient.HttpPost<PaymentsQueryRequest, PaymentsResponse>("v1/payments/query",
                request,accessTokens);
        }

        public Task<ImmutableList<BankProvider>> GetProviders()
        {
            return _aiiaHttpClient.HttpGet<ImmutableList<BankProvider>>("/v1/providers");
        }

        public async Task<AccountsResponse> GetUserAccounts(AiiaAccessTokens accessTokens)
        {
            return await _aiiaHttpClient.HttpGet<AccountsResponse>("/v1/accounts",accessTokens);
        }

        public async Task<InitiateDataUpdateResponse> InitiateDataUpdate(AiiaAccessTokens accessTokens, InitiateDataUpdateRequest requestBody)
        {
            return await _aiiaHttpClient.HttpPost<InitiateDataUpdateRequest, InitiateDataUpdateResponse>("v1/update",
                requestBody,
                accessTokens);
        }

        public async Task<Transaction> GetTransaction(AiiaAccessTokens accessTokens, string accountId, string transactionId)
        {
            return await _aiiaHttpClient.HttpGet<Transaction>($"/v1/accounts/{accountId}/transactions/{transactionId}",
                accessTokens);
        }
    }
}