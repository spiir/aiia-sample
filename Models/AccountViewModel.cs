using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using ViiaSample.Services;

namespace ViiaSample.Models
{
    public class AccountViewModel
    {
        public ILookup<string, Account> AccountsGroupedByProvider { get; set; }
        public string ViiaConnectUrl { get; set; }
        public JwtSecurityToken JwtToken { get; set; }
        public JwtSecurityToken RefreshToken { get; set; }
        public bool EmailEnabled { get; set; }
    }
}