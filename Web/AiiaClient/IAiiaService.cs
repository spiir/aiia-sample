using System;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading.Tasks;
using Aiia.Sample.Models;
using Aiia.Sample.Models.Aiia;
using Microsoft.AspNetCore.Http;

namespace Aiia.Sample.Services;

public interface IAiiaService
{
    Task<bool?> AllAccountsSelected(ClaimsPrincipal principal);
    Task<CreatePaymentResponse> CreateInboundPayment(ClaimsPrincipal user, CreatePaymentRequestViewModel body);

    Task<CreatePaymentResponse> CreateOutboundPayment(ClaimsPrincipal principal,
        CreatePaymentRequestViewModel request);

    Task<CreatePaymentResponseV2> CreatePaymentV2(ClaimsPrincipal principal,
        CreatePaymentRequestViewModelV2 requestViewModel);

    Task<CreatePaymentAuthorizationResponse> CreatePaymentAuthorization(ClaimsPrincipal principal,
        CreatePaymentAuthorizationRequestViewModel requestViewModel);

    Task<CodeExchangeResponse> ExchangeCodeForAccessToken(string code);

    Task<TransactionsResponse> GetAccountTransactions(ClaimsPrincipal principal,
        string accountId,
        TransactionQueryRequestViewModel queryRequest = null);

    Uri GetAuthUri(string userEmail);
    Task<InboundPayment> GetInboundPayment(ClaimsPrincipal principal, string accountId, string paymentId);
    Task<OutboundPayment> GetOutboundPayment(ClaimsPrincipal principal, string accountId, string paymentId);

    Task<PaymentAuthorization> GetPaymentAuthorization(ClaimsPrincipal principal, string accountId,
        string authorizationId);

    Task<PaymentsResponse> GetPayments(ClaimsPrincipal principal);
    Task<ImmutableList<BankProvider>> GetProviders();
    Task<IImmutableList<Account>> GetUserAccounts(ClaimsPrincipal principal);
    Task<InitiateDataUpdateResponse> InitiateDataUpdate(ClaimsPrincipal principal);

    Task ProcessWebHookPayload(HttpRequest request);
    Task<CodeExchangeResponse> RefreshAccessToken(string refreshToken);
}