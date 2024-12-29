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
        public GlassdoorWebscraper(string baseUrl, string apiKey, string dataSetId) : base(baseUrl, apiKey, dataSetId)
        {
            this.baseUrl = baseUrl;
            this.apiKey = apiKey;
            this.dataSetId = dataSetId;
        }

        public Task<SnapshotResponse> PerformScraping(List<GlassdoorRequest> glassdoorRequest)
        {
            string jsonRequest = JsonConvert.SerializeObject(glassdoorRequest);
            return base.PerformScraping(jsonRequest);
        }

        public async Task<List<GlassdoorResponse>> DownloadData(string snapshotId)
        {
            var glassdoorResponse = new List<GlassdoorResponse>();
            var options = new RestClientOptions(baseUrl);
            var client = new RestClient(options);
            var request = new RestRequest($"/datasets/v3/snapshot/{snapshotId}", Method.Get);
            request.AddHeader("Authorization", $"Bearer {apiKey}");
            RestResponse response = await client.ExecuteAsync(request);

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
