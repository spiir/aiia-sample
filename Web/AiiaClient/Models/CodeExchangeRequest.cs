namespace Aiia.Sample.AiiaClient.Models;

public class CodeExchangeRequest
{
    public string grant_type { get; set; }
    public string code { get; set; }
    public string scope { get; set; }
    public string redirect_uri { get; set; }
    public string refresh_token { get; set; }
}