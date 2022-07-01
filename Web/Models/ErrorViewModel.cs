namespace Aiia.Sample.Models;

public class ErrorViewModel
{
    public string RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public string ErrorMessage { get; set; }
    public string? ErrorStackTrace { get; set; }
}