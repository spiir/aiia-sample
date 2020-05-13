namespace ViiaSample.Models.Viia
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
