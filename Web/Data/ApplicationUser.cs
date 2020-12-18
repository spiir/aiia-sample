using System;
using Microsoft.AspNetCore.Identity;

namespace Aiia.Sample.Data
{
    public class ApplicationUser : IdentityUser
    {
        public bool EmailEnabled { get; set; }
        public string AiiaAccessToken { get; set; }
        public DateTimeOffset AiiaAccessTokenExpires { get; set; }
        public string AiiaConsentId { get; set; }
        public string AiiaRefreshToken { get; set; }
        public string AiiaTokenType { get; set; }
    }
}
