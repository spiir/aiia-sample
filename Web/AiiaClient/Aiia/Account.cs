using System;

namespace Aiia.Sample.Models.Aiia
{
    public class Account
    {
        public AccountProvider AccountProvider { get; set; }
        public Amount Available { get; set; }
        public Amount Booked { get; set; }
        public string Id { get; set; }
        public DateTime? LastSynchronized { get; set; }
        public string Name { get; set; }
        public AccountNumber Number { get; set; }
        public string Owner { get; set; }
        public string Type { get; set; }
    }
}