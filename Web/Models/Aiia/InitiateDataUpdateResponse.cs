namespace Aiia.Sample.Models.Aiia
{
    public class InitiateDataUpdateResponse
    {
        public string AuthUrl { get; set; }
        public UpdateStatus Status { get; set; }

        public enum UpdateStatus
        {
            AllQueued,
            SupervisedLoginRequired
        }
    }
}
