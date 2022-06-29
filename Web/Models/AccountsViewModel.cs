using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Aiia.Sample.Models.Aiia;

namespace Aiia.Sample.Models
{
    public class AccountsViewModel
    {
        public ILookup<string, Account> AccountsGroupedByProvider { get; set; }
        public string ConsentId { get; set; }
        public string Email { get; set; }
        public bool EmailEnabled { get; set; }
        public JwtSecurityToken JwtToken { get; set; }
        public IImmutableList<BankProvider> Providers { get; set; }
        public JwtSecurityToken RefreshToken { get; set; }
        public string AiiaConnectUrl { get; set; }
        public bool? AllAccountsSelected { get; set; }
    }
}