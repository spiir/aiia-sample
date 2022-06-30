namespace Aiia.Sample;

public class SiteOptions
{
    public ElasticSearchOptions ElasticSearch { get; set; }
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

public class ElasticSearchOptions
{
    public string IngestToken { get; set; }
    public string IngestUrl { get; set; }
}