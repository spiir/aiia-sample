using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;

namespace MyDataSample.Services
{
    public interface IMyDataService
    {
        Uri GetAuthUri(ClaimsPrincipal principal);
        Task<IEnumerable<Account>> GetUserAccounts(ClaimsPrincipal principal);
        Task<IEnumerable<Transaction>> GetAccountTransactions(string accountId);
    }
    
    public class MyDataService : IMyDataService
    {
        private readonly IOptionsMonitor<SiteOptions> _options;

        public MyDataService(IOptionsMonitor<SiteOptions> options)
        {
            _options = options;
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

        public Task<IEnumerable<Account>> GetUserAccounts(ClaimsPrincipal principal)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Transaction>> GetAccountTransactions(string accountId)
        {
            throw new System.NotImplementedException();
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
}