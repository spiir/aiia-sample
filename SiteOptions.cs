namespace MyDataSample
{
    public class SiteOptions
    {
        public MyDataOptions MyData { get; set; }
    }

    public class MyDataOptions
    {
        public string BaseAppUrl { get; set; }
        public string BaseApiUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string LoginCallbackUrl { get; set; }
    }
}