namespace AutomatedWebscraper.Domain.Response
{
    public class Candidate
    {
        public Content content { get; set; }
        public string finishReason { get; set; }
        public List<SafetyRating> safetyRatings { get; set; }
        public string avgLogprobs { get; set; }
    }

    public class Content
    {
        public List<Part> parts { get; set; }
        public string role { get; set; }
    }

    public class Part
    {
        public string text { get; set; }
    }

    public class GeminiResponseRoot
    {
        public List<Candidate> candidates { get; set; }
        public UsageMetadata usageMetadata { get; set; }
        public string modelVersion { get; set; }
    }

    public class SafetyRating
    {
        public string category { get; set; }
        public string probability { get; set; }
    }

    public class UsageMetadata
    {
        public int promptTokenCount { get; set; }
        public int candidatesTokenCount { get; set; }
        public int totalTokenCount { get; set; }
    }
}
