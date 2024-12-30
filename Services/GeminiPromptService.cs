using Polly;
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
        private readonly int timeOutInMin;
        public GeminiPromptService(string apiKey, string baseUrl,
            string modelName, int timeOutInMin)
        {
            this.apiKey = apiKey;
            this.baseUrl = baseUrl;
            this.modelName = modelName;
            this.timeOutInMin = timeOutInMin;
        }

        public async Task<GeminiResponseRoot> Execute(GeminiInputRoot geminiInputRoot)
        {
            var response = await Policy
                .HandleResult<RestResponse>(message => !message.IsSuccessStatusCode)
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(9)
                }, (result, timeSpan, retryCount, context) => {
                    Console.WriteLine($"Request failed with {result.Result.StatusCode}. " +
                        $"Retry count = {retryCount}. Waiting {timeSpan} before next retry. ");
                })
                .ExecuteAsync(async () =>
                {
                    var options = new RestClientOptions(baseUrl);
                    var client = new RestClient(options);
                    var request = new RestRequest($"/v1beta/models/{modelName}:generateContent?key={apiKey}",
                        Method.Post);

                    request.Timeout = TimeSpan.FromSeconds(double.Parse(timeOutInMin.ToString()));
                    request.AddHeader("Content-Type", "application/json");
                    var body = JsonConvert.SerializeObject(geminiInputRoot);
                    request.AddStringBody(body, DataFormat.Json);
                    RestResponse response = await client.ExecuteAsync(request, CancellationToken.None);
                    return response;
                });

            if (response.IsSuccessful)
            {
                return JsonConvert.DeserializeObject<GeminiResponseRoot>(response.Content);
            }

            return null;
        }
    }
}
