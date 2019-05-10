using System.Collections.Generic;
using ViiaSample.Services;

namespace ViiaSample.Models
{
    public class AccountViewModel
    {
        public List<Account> Accounts { get; set; }
        public string ViiaConnectUrl { get; set; }
    }
}