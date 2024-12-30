using Polly;
using RestSharp;
using Newtonsoft.Json;
using AutomatedWebscraper.Domain.Request;
using AutomatedWebscraper.Domain.Response;
using AutomatedWebscraper.Domain.Response.Glassdoor;

namespace AutomatedWebscraper.Webscraper
{
    public interface IGlassdoorWebscraper
    {
        Task<List<GlassdoorResponse>> DownloadData(string snapshotId);
        Task<SnapshotResponse> PerformScraping(List<GlassdoorRequest> glassdoorRequest);
    }

    /// <summary>
    /// Glassdoor Webscrapper
    /// </summary>
    public class GlassdoorWebscraper : BrightDataWebscraper, IGlassdoorWebscraper, IBrightDataWebscraper
    {
        private string apiKey;
        private string baseUrl;
        private string dataSetId;
        private int httpRequestTimeout;
        public GlassdoorWebscraper(string baseUrl, string apiKey, string dataSetId, int httpRequestTimeout) : base(baseUrl, apiKey, dataSetId, httpRequestTimeout)
        {
            this.baseUrl = baseUrl;
            this.apiKey = apiKey;
            this.dataSetId = dataSetId;
            this.httpRequestTimeout = httpRequestTimeout;
        }

        public Task<SnapshotResponse> PerformScraping(List<GlassdoorRequest> glassdoorRequest)
        {
            string jsonRequest = JsonConvert.SerializeObject(glassdoorRequest);
            return base.PerformScraping(jsonRequest);
        }

        public async Task<List<GlassdoorResponse>> DownloadData(string snapshotId)
        {
            var glassdoorResponse = new List<GlassdoorResponse>();
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
                    options.Timeout = TimeSpan.FromSeconds(httpRequestTimeout);
                    var client = new RestClient(options);
                    var request = new RestRequest($"/datasets/v3/snapshot/{snapshotId}", Method.Get);
                    request.AddHeader("Authorization", $"Bearer {apiKey}");
                    RestResponse response = await client.ExecuteAsync(request);
                    return response;
                });

            if (response.IsSuccessful)
            {
                string uniqueFile = $"{Guid.NewGuid().ToString()}.json";
                System.IO.File.WriteAllText(uniqueFile, response.Content);

                foreach (var line in File.ReadLines(uniqueFile))
                {
                    var jsonResponse = JsonConvert.DeserializeObject<GlassdoorResponse>(line);
                    glassdoorResponse.Add(jsonResponse);
                }

                System.IO.File.Delete(uniqueFile);
                return glassdoorResponse;
            }

            return glassdoorResponse;
        }
    }
}
