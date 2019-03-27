using System;
using Microsoft.AspNetCore.Identity;

namespace MyDataSample.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string MyDataAccessToken { get; set; }
        public string MyDataRefreshToken { get; set; }
        public DateTimeOffset MyDataAccessTokenExpires { get; set; }
        public string MyDataTokenType { get; set; }
    }
}