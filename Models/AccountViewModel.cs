using System.Collections.Generic;
using System.Linq;
using ViiaSample.Services;

namespace ViiaSample.Models
{
    public class AccountViewModel
    {
        public ILookup<string, Account> AccountsGroupedByProvider { get; set; }
        public string ViiaConnectUrl { get; set; }
    }
}