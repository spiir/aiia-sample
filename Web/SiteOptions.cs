namespace Aiia.Sample
{
    public class SiteOptions
    {
        public HumioOptions Humio { get; set; }
        public bool LogToConsole { get; set; } = false;
        public SendGridOptions SendGrid { get; set; }
        public AiiaOptions Aiia { get; set; }
    }

    public class AiiaOptions
    {
        public string BaseApiUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string WebHookSecret { get; set; }
    }

    public class SendGridOptions
    {
        public string ApiKey { get; set; }
        public string EmailFrom { get; set; }
        public string NameFrom { get; set; }
    }

    public class HumioOptions
    {
        public string IngestToken { get; set; }
        public string IngestUrl { get; set; }
    }
}