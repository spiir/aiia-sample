using System;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading.Tasks;
using Aiia.Sample.AiiaClient.Models;
using Aiia.Sample.AiiaClient.Models.V2;
using Aiia.Sample.Models;
using Aiia.Sample.Models.V2;
using Microsoft.AspNetCore.Http;

namespace Aiia.Sample.AiiaClient;

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

    Task ExchangeCodeForAccessToken(ClaimsPrincipal principal, string code, string consentId);

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
    Task<IImmutableList<Account>> GetAccounts(ClaimsPrincipal principal);
    Task<InitiateDataUpdateResponse> InitiateDataUpdate(ClaimsPrincipal principal);

    Task ProcessWebHookPayload(HttpRequest request);
}