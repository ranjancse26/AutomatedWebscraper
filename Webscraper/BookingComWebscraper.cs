using Polly;
using RestSharp;
using Newtonsoft.Json;
using AutomatedWebscraper.Domain.Response.BookingCom;
using AutomatedWebscraper.Domain.Request;
using AutomatedWebscraper.Domain.Response;

namespace AutomatedWebscraper.Webscraper
{
    public interface IBookingComWebscraper
    {
        Task<List<BookingResponse>> DownloadData(string snapshotId);
        Task<SnapshotResponse> PerformScraping(List<BookingComRequest> bookingComRequest);
    }

    /// <summary>
    /// Booking.com Webscrapper
    /// </summary>
    public class BookingComWebscraper : BrightDataWebscraper, IBookingComWebscraper, IBrightDataWebscraper
    {
        private string apiKey;
        private string baseUrl;
        private string dataSetId;
        public int httpRequestTimeout;
        public BookingComWebscraper(string baseUrl, string apiKey, string dataSetId, int httpRequestTimeout) : base(baseUrl, apiKey, dataSetId, httpRequestTimeout)
        {
            this.baseUrl = baseUrl;
            this.apiKey = apiKey;
            this.dataSetId = dataSetId;
            this.httpRequestTimeout = httpRequestTimeout;
        }

        public Task<SnapshotResponse> PerformScraping(List<BookingComRequest> bookingComRequest)
        {
            string jsonBookingComRequest = JsonConvert.SerializeObject(bookingComRequest);
            return base.PerformScraping(jsonBookingComRequest);
        }

        public async Task<List<BookingResponse>> DownloadData(string snapshotId)
        {
            var bookingResponses = new List<BookingResponse>();
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
                    var bookingResponse = JsonConvert.DeserializeObject<BookingResponse>(line);
                    bookingResponses.Add(bookingResponse);
                }

                System.IO.File.Delete(uniqueFile);
                return bookingResponses;
            }

            return bookingResponses;
        }
    }
}
