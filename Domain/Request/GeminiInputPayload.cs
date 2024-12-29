namespace AutomatedWebscraper.Domain.Request
{
    public class Content
    {
        public string role { get; set; }
        public List<Part> parts { get; set; }
    }

    public class GenerationConfig
    {
        public int temperature { get; set; }
        public int topK { get; set; }
        public double topP { get; set; }
        public int maxOutputTokens { get; set; }
        public string responseMimeType { get; set; }
    }

    public class Part
    {
        public string text { get; set; }
    }

    public class GeminiInputRoot
    {
        public List<Content> contents { get; set; }
        public SystemInstruction systemInstruction { get; set; }
        public GenerationConfig generationConfig { get; set; }
    }

    public class SystemInstruction
    {
        public string role { get; set; }
        public List<Part> parts { get; set; }
    }
}
