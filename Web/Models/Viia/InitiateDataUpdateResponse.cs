namespace ViiaSample.Models.Viia
{
    public class InitiateDataUpdateResponse
    {
        public UpdateStatus Status { get; set; }
        public string AuthUrl { get; set; }
        
        public enum UpdateStatus
        {
            AllQueued,
            SupervisedLoginRequired
        }
    }
}