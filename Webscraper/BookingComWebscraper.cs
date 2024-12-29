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
        public BookingComWebscraper(string baseUrl, string apiKey, string dataSetId) : base(baseUrl, apiKey, dataSetId)
        {
            this.baseUrl = baseUrl;
            this.apiKey = apiKey;
            this.dataSetId = dataSetId;
        }

        public Task<SnapshotResponse> PerformScraping(List<BookingComRequest> bookingComRequest)
        {
            string jsonBookingComRequest = JsonConvert.SerializeObject(bookingComRequest);
            return base.PerformScraping(jsonBookingComRequest);
        }

        public async Task<List<BookingResponse>> DownloadData(string snapshotId)
        {
            var bookingResponses = new List<BookingResponse>();
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
