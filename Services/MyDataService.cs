using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MyDataSample.Services
{
    public interface IMyDataService
    {
        Task<Uri> GetAuthUri(ClaimsPrincipal principal);
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

        public Task<Uri> GetAuthUri(ClaimsPrincipal principal)
        {
            // Something like POST MyData.API/start {client credentials + user hash}
            // Returns MyData.APP url with some token probably
            // return it here
            
            throw new System.NotImplementedException();
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
        
    }

    public class Transaction
    {
        
    }
}