namespace AutomatedWebscraper.Domain.Response.Glassdoor
{
    public class GlassdoorFinetuneInput
    {
        public string company { get; set; }
        public string overview_url { get; set; }
    }

    public class GlassdoorFinetuneResponse
    {
        public string instruction { get; set; }
        public GlassdoorFinetuneInput input { get; set; }
        public string output { get; set; }
    }
}
