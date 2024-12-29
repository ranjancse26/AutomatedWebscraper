namespace AutomatedWebscraper.Domain.Request
{
    public class BookingComRequest
    {
        public string url { get; set; }
        public string location { get; set; }
        public DateTime check_in { get; set; }
        public DateTime check_out { get; set; }
        public int adults { get; set; }
        public int rooms { get; set; }
    }
}
