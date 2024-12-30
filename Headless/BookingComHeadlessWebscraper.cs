using System.Text;
using PuppeteerSharp;
using System.Net.WebSockets;

namespace AutomatedWebscraper.Headless
{
    public interface IBookingComHeadlessWebscraper
    {
        Task<string> PerformScraping(string city, string checkIn, string checkOut);
    }

    /// <summary>
    /// Booking.com Headless Webscrapper
    /// </summary>
    public class BookingComHeadlessWebscraper : IBookingComHeadlessWebscraper
    {
        private const string URL = "https://www.booking.com/";
        private readonly string wssEndpoint;
        private readonly int popupTimeout;
        public BookingComHeadlessWebscraper(string wssEndpoint, int popupTimeout)
        {
            this.wssEndpoint = wssEndpoint;
            this.popupTimeout = popupTimeout;
        }

        async Task ClosePopup(IPage page)
        {
            try
            {
                var closeBtn = await page.WaitForSelectorAsync("[aria-label='Dismiss sign-in info.']", new WaitForSelectorOptions
                {
                    Timeout = popupTimeout,
                    Visible = true
                });
                Console.WriteLine("Popup appeared! Closing...");
                await closeBtn.ClickAsync();
                Console.WriteLine("Popup closed!");
            }
            catch (PuppeteerException)
            {
                Console.WriteLine("Popup didn't appear.");
            }
        }

        async Task Interact(IPage page, string city, string checkIn, string checkOut)
        {
            Console.WriteLine("Waiting for search form...");
            var searchInput = await page.WaitForSelectorAsync("[data-testid='destination-container'] input", new WaitForSelectorOptions
            {
                Timeout = 80000
            });

            Console.WriteLine("Search form appeared! Filling it...");
            await searchInput.TypeAsync(city);

            await page.ClickAsync("[data-testid='searchbox-dates-container'] button");
            await page.WaitForSelectorAsync("[data-testid='searchbox-datepicker-calendar']");

            await page.ClickAsync($"[data-date='{checkIn}']");
            await page.ClickAsync($"[data-date='{checkOut}']");

            Console.WriteLine("Form filled! Submitting and waiting for result...");
            await Task.WhenAll(
                page.ClickAsync("button[type='submit']"),
                page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded } })
            );
        }

        async Task<List<PropertyData>> Parse(IPage page)
        {
            var results = await page.EvaluateFunctionAsync<List<PropertyData>>(@"
            () => {
                return Array.from(document.querySelectorAll('[data-testid=""property-card""]')).map(el => {
                    const name = el.querySelector('[data-testid=""title""]')?.innerText || null;
                    const price = el.querySelector('[data-testid=""price-and-discounted-price""]')?.innerText || null;
                    const reviewScore = el.querySelector('[data-testid=""review-score""]')?.innerText || '';
                    const [scoreStr, , , reviewsStr = ''] = reviewScore.split('\n');
                    const score = parseFloat(scoreStr) || scoreStr;
                    const reviews = parseInt(reviewsStr.replace(/\D/g, '')) || reviewsStr;
                    return { name, price, score, reviews };
                });
            }"
            );

            return results ?? new List<PropertyData>();
        }

        public async Task<string> PerformScraping(string city, string checkIn, string checkOut)
        {
            Console.WriteLine("Connecting to browser...");
            await new BrowserFetcher().DownloadAsync();

            var Connect = (string ws) => Puppeteer.ConnectAsync(new()
            {
                BrowserWSEndpoint = ws,
                WebSocketFactory = async (url, options, cToken) =>
                {
                    var socket = new ClientWebSocket();
                    var authBytes = Encoding.UTF8.GetBytes(new Uri(ws).UserInfo);
                    var authHeader = "Basic " + Convert.ToBase64String(authBytes);
                    socket.Options.SetRequestHeader("Authorization", authHeader);
                    socket.Options.KeepAliveInterval = TimeSpan.Zero;
                    await socket.ConnectAsync(url, cToken);
                    return socket;
                },
            });

            Console.WriteLine("Connecting to Scraping Browser...");
            using var browser = await Connect(wssEndpoint);
            Console.WriteLine("Connected! Navigating to site...");

            var page = await browser.NewPageAsync();
            await page.GoToAsync(URL, WaitUntilNavigation.DOMContentLoaded);
            Console.WriteLine("Navigated! Waiting for popup...");

            await ClosePopup(page);
            Thread.Sleep(2000);

            await Interact(page, city, checkIn, checkOut);
            Console.WriteLine("Parsing data...");

            var data = await Parse(page);
            var parsedData = System.Text.Json.JsonSerializer.Serialize(data,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            await browser.CloseAsync();
            return parsedData;
        }

        public class PropertyData
        {
            public string Name { get; set; }
            public string Price { get; set; }
            public object Score { get; set; }
            public object Reviews { get; set; }
        }
    }
}
