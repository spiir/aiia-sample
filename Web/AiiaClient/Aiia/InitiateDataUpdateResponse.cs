namespace Aiia.Sample.Models.Aiia;

public class InitiateDataUpdateResponse
{
    public enum UpdateStatus
    {
        AllQueued,
        SupervisedLoginRequired
    }

    public string AuthUrl { get; set; }
    public UpdateStatus Status { get; set; }
}