namespace AutomatedWebscraper.Domain.Response
{
    public class MonitorStatus
    {
        public string status { get; set; }
        public string snapshot_id { get; set; }
        public string dataset_id { get; set; }
        public int records { get; set; }
        public int errors { get; set; }
        public int collection_duration { get; set; }
    }
}
