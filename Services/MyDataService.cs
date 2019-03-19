using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

    public class MockMyDataService : IMyDataService
    {
        private readonly IHttpContextAccessor _context;

        public MockMyDataService(IHttpContextAccessor context)
        {
            _context = context;
        }

        public Task<Uri> GetAuthUri(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException();
            }
            var currentBaseUrl = $"{_context.HttpContext.Request.Scheme}://{_context.HttpContext.Request.Host}{_context.HttpContext.Request.PathBase}";
            return Task.FromResult(new Uri(new Uri(currentBaseUrl), "mydata/mock"));
        }

        public Task<IEnumerable<Account>> GetUserAccounts(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException();
            }
            
            var rnd = new Random(principal.GetHashCode());
            var resultList = new List<Account>();
            for (var i = 0; i < rnd.Next(1, 5); i++)
            {
                resultList.Add(RandomAccount(principal));
            }
            
            return Task.FromResult((IEnumerable<Account>) resultList);
        }

        public Task<IEnumerable<Transaction>> GetAccountTransactions(string accountId)
        {
            var rnd = new Random();
            var resultList = new List<Transaction>();
            for (var i = 0; i < rnd.Next(30, 100); i++)
            {
                resultList.Add(RandomTransaction(accountId));
            }

            return Task.FromResult((IEnumerable<Transaction>) resultList);
        }

        private Account RandomAccount(ClaimsPrincipal principal)
        {
            var nameIdentifierClaim = principal.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier));
            if (nameIdentifierClaim == null)
            {
                throw new ArgumentNullException();
            }

            var types = new[] {"Personal", "Business", "Savings"};
            var rnd = new Random();

            var bban = $"{rnd.Next(1000, 9999)}{rnd.Next(1000, 9999)}{rnd.Next(1000, 9999)}";
            var bankCode = rnd.Next(10, 99);
            return new Account()
            {
                Id = Guid.NewGuid().ToString(),
                ProviderId = Guid.NewGuid().ToString(),
                Name = $"{types[rnd.Next(0, types.Length)]} account",
                Number = new BankNumber()
                {
                    BbanType = "Bban",
                    Bban = bban,
                    Iban = $"DK{bankCode}{bban}",
                    BbanParsed = new BbanParsed()
                    {
                        AccountNumber = bban,
                        BankCode = bankCode.ToString()
                    }
                },
                AvailableBalance = $"{rnd.Next(3000, 100000)}",
                BookedBalance = $"{rnd.Next(0, 3000)}",
                Currency = rnd.Next(3) % 2 == 0 ? "DKK" : "EUR",
                IsPaymentAccount = rnd.Next(9) % 3 == 0,
                Type = types[rnd.Next(0, types.Length)]
            };
        }

        private Transaction RandomTransaction(string accountId)
        {
            var texts = new[] {"Netto", "Netflix", "Spotify", "Gas", "Water", "Rent"};
            var rnd = new Random();
            var chosenText = texts[rnd.Next(0, texts.Length)];
            var date = DateTimeOffset.UtcNow.AddMinutes(-rnd.Next(1, 10000));
            return new Transaction()
            {
                Amount = rnd.Next(10, 20000),
                CreationDate = date,
                Currency = rnd.Next(3) % 2 == 0 ? "DKK" : "EUR",
                Date = date.AddMinutes(rnd.Next(3, 100)),
                Id = Guid.NewGuid().ToString(),
                OriginalText = chosenText,
                State = "Executed",
                Text = chosenText,
                Type = "Payment"
            };
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