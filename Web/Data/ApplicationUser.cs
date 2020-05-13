using System;
using Microsoft.AspNetCore.Identity;

namespace ViiaSample.Data
{
    public class ApplicationUser : IdentityUser
    {
        public bool EmailEnabled { get; set; }
        public string ViiaAccessToken { get; set; }
        public DateTimeOffset ViiaAccessTokenExpires { get; set; }
        public string ViiaConsentId { get; set; }
        public string ViiaRefreshToken { get; set; }
        public string ViiaTokenType { get; set; }
    }
}
