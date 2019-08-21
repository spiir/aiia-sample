using System;

namespace ViiaSample.Models.Viia
{
    public class Account
    {
        public Amount Available { get; set; }
        public Amount Booked { get; set; }
        public string Id { get; set; }
        public AccountProvider Provider { get; set; }
        public string Name { get; set; }
        public AccountNumber Number { get; set; }
        public string Type { get; set; }
        public DateTime? LastSynchronized { get; set; }
        public string Owner { get; set; }
    }
}