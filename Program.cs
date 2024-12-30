using Serilog;
using Newtonsoft.Json;
using System.Reflection;
using Serilog.Sinks.SystemConsole.Themes;
using AutomatedWebscraper.Constant;
using AutomatedWebscraper.Domain.Request;
using AutomatedWebscraper.Services;
using Microsoft.Extensions.Configuration;
using AutomatedWebscraper.Domain.Response.Glassdoor;

class BookingScraper
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .CreateLogger();

        var builder = new ConfigurationBuilder()
               .AddJsonFile($"appSettings.json", true, true);

        IConfigurationRoot config = builder.Build();
        string brightDataBaseUrl = config[AppConstant.BrightDataBaseUrl];
        string wssEndpoint = config[AppConstant.WSSBrowserCredential];
        string webscraperApiKey = config[AppConstant.WebscraperApiToken];
        string geminiApiKey = config[LLMConstant.GeminiApiKey];
        int popupTimeout = int.Parse(config[AppConstant.PopupTimeout]);
        int httpRequestTimeout = int.Parse(config[AppConstant.HttpRequestTimeout]);


        string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string promptsFolderPath = Path.Combine(exePath, "Prompts");
        string glassdoorSummaryPrompt = File.ReadAllText($"{promptsFolderPath}//GlassdoorCompanySummaryPrompt.json");

        var geminiPromptService = new GeminiPromptService(geminiApiKey,
            "https://generativelanguage.googleapis.com", "gemini-2.0-flash-exp", httpRequestTimeout);
        var glassdoorService = new GlassdoorService(glassdoorSummaryPrompt, geminiPromptService, httpRequestTimeout);

        IBookingComService bookingComService = new BookingComService(brightDataBaseUrl, webscraperApiKey, httpRequestTimeout);

        System.Console.WriteLine("1. Booking Headless Browser Webscraper");
        System.Console.WriteLine("2. Booking Webscraper API based Webscraper");
        System.Console.WriteLine("3. Glassdoor Company Info using Webscraper API");
        System.Console.WriteLine("4. Glassdoor Company Info using Web Data");

        System.Console.WriteLine("\nPlease enter your choice: ");

        string choice = System.Console.ReadLine();
        switch (choice)
        {
            case "1":
                string city = "New York";
                var now = DateTime.Now;
                var checkIn = bookingComService.ToBookingTimestamp(
                    bookingComService.AddDays(now, 1));
                var checkOut = bookingComService.ToBookingTimestamp(
                    bookingComService.AddDays(now, 2));
                await bookingComService.PerformBookingHeadlessBrowserScraping(city, wssEndpoint, popupTimeout, checkIn, checkOut);
                break;
            case "2":
                List<BookingComRequest> bookingComRequests =
                [
                    new BookingComRequest
                    {
                        url = "https://www.booking.com",
                        location = "Tel Aviv",
                        check_in = DateTime.Parse("2025-02-01"),
                        check_out = DateTime.Parse("2025-02-10"),
                        adults = 2,
                        rooms = 1
                    },
                    new BookingComRequest
                    {
                        url = "https://www.booking.com",
                        location = "Paris",
                        check_in = DateTime.Parse("2025-04-30"),
                        check_out = DateTime.Parse("2025-05-07"),
                        adults = 2,
                        rooms = 1
                    },
                ];

                var bookingData = await bookingComService.PerformBookingComApiScraping(bookingComRequests);

                if (bookingData.Count > 0)
                    System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(bookingData,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                else
                    System.Console.WriteLine("No booking data");
                break;
            case "3":
                string urlToScrape = "https://www.glassdoor.co.uk/Overview/Working-at-Bright-Data-EI_IE2267280.11,22.htm";
                var structuredInfo = await glassdoorService.PerformGlassdoorApiScraping(urlToScrape, brightDataBaseUrl, webscraperApiKey,
                    geminiApiKey, "");
                if (!string.IsNullOrEmpty(structuredInfo))
                {
                    var fineTuneInstructionSet = glassdoorService.GetFinetuneResponse("Bright Data",
                        urlToScrape, structuredInfo);
                    System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(fineTuneInstructionSet,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                }
                break;
            case "4":
                string webDataFolderPath = Path.Combine(exePath, "WebData");
                string glassdoorWebData = File.ReadAllText($"{webDataFolderPath}//Glassdoor-Bright-Data.json");
                var glassdoorResponses = JsonConvert.DeserializeObject<List<GlassdoorResponse>>(glassdoorWebData);
                foreach(var glassdoorResponse in glassdoorResponses)
                {
                    var structuredResponse = await glassdoorService.GetStructuredInfo(glassdoorResponse);
                    if (!string.IsNullOrEmpty(structuredResponse))
                    {
                        var fineTuneInstructionSet = glassdoorService.GetFinetuneResponse("Bright Data",
                            "https://www.glassdoor.co.uk/Overview/Working-at-Bright-Data-EI_IE2267280.11,22.htm",
                            structuredResponse);
                        System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(fineTuneInstructionSet,
                            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    }
                }
                break;
            default:
                System.Console.WriteLine("Invalid option. Please try again!");
                break;
        }

        System.Console.WriteLine("Press any key to exit!");
        System.Console.ReadLine();
    }
}
