using Serilog;
using AutomatedWebscraper.Constant;
using AutomatedWebscraper.Domain.Request;
using AutomatedWebscraper.Domain.Response;
using AutomatedWebscraper.Headless;
using AutomatedWebscraper.Webscraper;
using AutomatedWebscraper.Domain.Response.BookingCom;

namespace AutomatedWebscraper.Services
{
    public interface IBookingComService
    {
        DateTime AddDays(DateTime date, int days);
        string ToBookingTimestamp(DateTime date);
        Task<List<BookingResponse>> PerformBookingComApiScraping(List<BookingComRequest> bookingComRequests);
        Task<List<BookingResponse>> PerformBookingComApiScraping(string snapshotId);
        Task PerformBookingHeadlessBrowserScraping(string city,
            string wssEndpoint, int popupTimeout, string checkIn, string checkOut);
    }

    /// <summary>
    /// Booking.com Service
    /// </summary>
    public class BookingComService : IBookingComService
    {
        private string baseUrl;
        private string apiKey;
        private int httpRequestTimeout;
        public BookingComService(string baseUrl, string apiKey, int httpRequestTimeout) 
        {
            this.baseUrl = baseUrl;
            this.apiKey = apiKey;
            this.httpRequestTimeout = httpRequestTimeout;
        }


        public DateTime AddDays(DateTime date, int days)
        {
            return date.AddDays(days);
        }

        public string ToBookingTimestamp(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }


        public async Task<List<BookingResponse>> PerformBookingComApiScraping(List<BookingComRequest> bookingComRequests)
        {
            try
            {
                // Web Scraper API
                string dataSetId = BrightDatasetConstant.BookingComDatasetId;
                IBookingComWebscraper bookingComWebscraper = new BookingComWebscraper(baseUrl, apiKey, dataSetId, httpRequestTimeout);

                var snapshotResponse = await bookingComWebscraper.PerformScraping(bookingComRequests);
                System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(snapshotResponse,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

                if (snapshotResponse != null &&
                    !string.IsNullOrEmpty(snapshotResponse.snapshot_id))
                {
                    MonitorStatus monitorStatus;
                    do
                    {
                        monitorStatus = await ((IBrightDataWebscraper)bookingComWebscraper)
                            .GetMonitorStatus(snapshotResponse.snapshot_id);
                        System.Console.WriteLine($"Monitor Status: {monitorStatus.status}");
                        System.Threading.Thread.Sleep(1000);
                    }
                    while (monitorStatus != null && monitorStatus.status != "ready");

                    var bookingData = await bookingComWebscraper.DownloadData(snapshotResponse.snapshot_id);
                    return bookingData;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception occured in PerformBookingComApiScraping. {Ex}", ex);
            }
            return new List<BookingResponse>();
        }


        public async Task<List<BookingResponse>> PerformBookingComApiScraping(string snapshotId)
        {
            try
            {
                // Web Scraper API
                string dataSetId = BrightDatasetConstant.BookingComDatasetId;
                IBookingComWebscraper bookingComWebscraper = new BookingComWebscraper(baseUrl, apiKey, dataSetId, httpRequestTimeout);

                var monitorStatus = await ((IBrightDataWebscraper)bookingComWebscraper).GetMonitorStatus(snapshotId);

                if (monitorStatus != null && monitorStatus.status == "ready")
                {
                    var bookingData = await bookingComWebscraper.DownloadData(snapshotId);
                    return bookingData;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception occured in PerformBookingComApiScraping. {Ex}", ex);
            }
            return new List<BookingResponse>();
        }

        public async Task PerformBookingHeadlessBrowserScraping(string city,
            string wssEndpoint, int popupTimeout, string checkIn, string checkOut)
        {
            try
            {
                // Headless Browser
                BookingComHeadlessWebscraper bookingComHeadlessWebscraper =
                    new BookingComHeadlessWebscraper(wssEndpoint, popupTimeout);

                var jsonData = await bookingComHeadlessWebscraper.PerformScraping(city, checkIn, checkOut);
                Console.WriteLine($"Booking Response: {jsonData}");
            }
            catch (Exception ex)
            {
                Log.Error("Exception occured in PerformBookingHeadlessBrowserScraping. {Ex}", ex);
            }
        }
    }
}
