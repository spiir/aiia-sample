using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using ViiaSample.Models.Viia;

namespace ViiaSample.Models
{
    public class AccountsViewModel
    {
        public ILookup<string, Account> AccountsGroupedByProvider { get; set; }
        public bool EmailEnabled { get; set; }
        public JwtSecurityToken JwtToken { get; set; }
        public IImmutableList<BankProvider> Providers { get; set; }
        public JwtSecurityToken RefreshToken { get; set; }
        public string ViiaConnectUrl { get; set; }
        public string ViiaOneTimeConnectUrl { get; set; }
    }
}
