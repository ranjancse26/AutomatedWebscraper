using RestSharp;
using Newtonsoft.Json;
using AutomatedWebscraper.Domain.Response;

namespace AutomatedWebscraper.Webscraper
{
    public interface IBrightDataWebscraper
    {
        Task<MonitorStatus> GetMonitorStatus(string snapshotId);
        Task<bool> CancelSnapshot(string snapshotId);
        Task<SnapshotResponse> PerformScraping(string data, bool includeErrors=true);
    }


    /// <summary>
    /// Abstract Bright Data Webscrapper
    /// </summary>
    public abstract class BrightDataWebscraper : IBrightDataWebscraper
    {
        private string apiKey;
        private string dataSetId;
        private string baseUrl;
        public BrightDataWebscraper(string baseUrl, string apiKey, string dataSetId)
        {
            this.baseUrl = baseUrl;
            this.apiKey = apiKey;
            this.dataSetId = dataSetId;
        }
        
        public async Task<bool> CancelSnapshot(string snapshotId)
        {
            var options = new RestClientOptions(baseUrl);
            var client = new RestClient(options);
            var request = new RestRequest($"/datasets/v3/snapshot/{snapshotId}/cancel", Method.Post);
            request.AddHeader("Authorization", $"Bearer {apiKey}");
            RestResponse restResponse = await client.ExecuteAsync(request);
            if (restResponse.IsSuccessful)
            {
                var response = JsonConvert.DeserializeObject<string>(restResponse.Content);
                if (response == "OK")
                    return true;
            }

            return false;
        }

        public async Task<MonitorStatus> GetMonitorStatus(string snapshotId)
        {
            var options = new RestClientOptions(baseUrl);
            var client = new RestClient(options);
            var request = new RestRequest($"/datasets/v3/progress/{snapshotId}", Method.Get);
            request.AddHeader("Authorization", $"Bearer {apiKey}");
            RestResponse response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                var monitorStatus = JsonConvert.DeserializeObject<MonitorStatus>(response.Content);
                return monitorStatus;
            }

            return null;
        }


        public async Task<SnapshotResponse> PerformScraping(string data, bool includeErrors = true)
        {
            var options = new RestClientOptions(baseUrl);
            var client = new RestClient(options);
            string requestingUrl = $"/datasets/v3/trigger?dataset_id={dataSetId}";

            if (includeErrors)
                requestingUrl += "&include_errors=true";

            var request = new RestRequest(requestingUrl, Method.Post);

            request.AddHeader("Authorization", $"Bearer {apiKey}");
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody(data, DataFormat.Json);
            RestResponse response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                var snapshotResponse = JsonConvert.DeserializeObject<SnapshotResponse>(response.Content);
                return snapshotResponse;
            }

            return null;
        }
    }
}
