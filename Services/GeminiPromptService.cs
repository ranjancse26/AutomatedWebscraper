using RestSharp;
using Newtonsoft.Json;
using AutomatedWebscraper.Domain.Response;
using AutomatedWebscraper.Domain.Request;

namespace AutomatedWebscraper.Services
{
    public interface IGeminiPromptService
    {
        Task<GeminiResponseRoot> Execute(GeminiInputRoot geminiInputRoot);
    }

    /// <summary>
    /// Gemini Prompt Service
    /// </summary>
    public class GeminiPromptService : IGeminiPromptService
    {
        private readonly string apiKey;
        private readonly string baseUrl;
        private readonly string modelName;
        private readonly string timeOutInMin;
        public GeminiPromptService(string apiKey, string baseUrl,
            string modelName, string timeOutInMin)
        {
            this.apiKey = apiKey;
            this.baseUrl = baseUrl;
            this.modelName = modelName;
            this.timeOutInMin = timeOutInMin;
        }

        public async Task<GeminiResponseRoot> Execute(GeminiInputRoot geminiInputRoot)
        {
            try
            {
                var options = new RestClientOptions(baseUrl);
                var client = new RestClient(options);
                var request = new RestRequest($"/v1beta/models/{modelName}:generateContent?key={apiKey}",
                    Method.Post);

                request.Timeout = TimeSpan.FromMinutes(double.Parse(timeOutInMin));
                request.AddHeader("Content-Type", "application/json");
                var body = JsonConvert.SerializeObject(geminiInputRoot);
                request.AddStringBody(body, DataFormat.Json);
                RestResponse response = await client.ExecuteAsync(request, CancellationToken.None);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<GeminiResponseRoot>(response.Content);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
