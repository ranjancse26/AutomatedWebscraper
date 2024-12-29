using Serilog;
using System.Net;
using Newtonsoft.Json;
using AutomatedWebscraper.Constant;
using AutomatedWebscraper.Webscraper;
using AutomatedWebscraper.Domain.Request;
using AutomatedWebscraper.Domain.Response;
using AutomatedWebscraper.Domain.Response.Glassdoor;

namespace AutomatedWebscraper.Services
{
    public interface IGlassdoorService
    {
        Task<string> GetStructuredInfo(GlassdoorResponse glassdoorResponse);
        Task<string> PerformGlassdoorApiScraping(string urlToScrape, string brightDataBaseUrl, string apiKey,
            string geminiApiKey, string snapshotId);
        GlassdoorFinetuneResponse GetFinetuneResponse(string company, string url,
            string structuredResponse);
    }

    /// <summary>
    /// Glassdoor Service
    /// </summary>
    public class GlassdoorService : IGlassdoorService
    {
        private string promptPayload;
        private readonly IGeminiPromptService geminiPromptService;
        private const string Instruction = "Generate a professional company summary based on the provided data.";

        public GlassdoorService(string promptPayload, IGeminiPromptService geminiPromptService)
        {
            this.promptPayload = promptPayload;
            this.geminiPromptService = geminiPromptService;
        }

        /// <summary>
        /// Get Structured Information
        /// </summary>
        /// <param name="glassdoorResponse">Collection of GlassdoorResponse</param>
        /// <returns>Structured Glassdoor Response</returns>
        public async Task<string> GetStructuredInfo(GlassdoorResponse glassdoorResponse)
        {
            var jsonGlassdoorResponse = JsonConvert.SerializeObject(glassdoorResponse);
            string encodedGlassdoorResponse = WebUtility.HtmlEncode(jsonGlassdoorResponse);
            promptPayload = promptPayload.Replace("{{content}}", encodedGlassdoorResponse);
            var payload = JsonConvert.DeserializeObject<GeminiInputRoot>(promptPayload);
            GeminiResponseRoot geminiResponseRoot = await geminiPromptService.Execute(payload);

            if (geminiResponseRoot != null)
            {
                if (geminiResponseRoot.candidates.Count > 0 &&
                   geminiResponseRoot.candidates[0].content != null &&
                   geminiResponseRoot.candidates[0].content.parts.Count > 0)
                {
                    return geminiResponseRoot.candidates[0].content.parts[0].text;
                }
            }

            return string.Empty;
        }

        public GlassdoorFinetuneResponse GetFinetuneResponse(string company, string url,
            string structuredResponse)
        {
            GlassdoorFinetuneResponse glassdoorFinetuneResponse = new GlassdoorFinetuneResponse
            {
                instruction = Instruction,
                input = new GlassdoorFinetuneInput
                {
                    company = company,
                    overview_url = url,
                },
                output = structuredResponse
            };
            return glassdoorFinetuneResponse;
        }


        public async Task<string> PerformGlassdoorApiScraping(string urlToScrape, string brightDataBaseUrl, string apiKey,
            string geminiApiKey, string snapshotId)
        {
            string dataSetId = BrightDatasetConstant.GlassdoorDatasetId;
            IGlassdoorWebscraper glassdoorWebscraper = new GlassdoorWebscraper(brightDataBaseUrl, apiKey, dataSetId);

            if (string.IsNullOrEmpty(snapshotId))
            {
                var snapshotResponse = await glassdoorWebscraper.PerformScraping(new List<GlassdoorRequest>
                {
                   new GlassdoorRequest
                   {
                       url = urlToScrape
                   }
                });

                System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(snapshotResponse,
                   new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

                if (snapshotResponse != null &&
                    !string.IsNullOrEmpty(snapshotResponse.snapshot_id))
                {
                    return await ProcessGlassdoorRequest(glassdoorWebscraper, snapshotResponse);
                }
            }
            else
            {
                return await ProcessGlassdoorRequest(glassdoorWebscraper, new SnapshotResponse
                {
                    snapshot_id = snapshotId
                });
            }

            return string.Empty;
        }

        private async Task<string> ProcessGlassdoorRequest(IGlassdoorWebscraper glassdoorWebscraper,
            SnapshotResponse snapshotResponse)
        {
            try
            {
                MonitorStatus monitorStatus;
                do
                {
                    monitorStatus = await ((IBrightDataWebscraper)glassdoorWebscraper)
                        .GetMonitorStatus(snapshotResponse.snapshot_id);
                    System.Console.WriteLine($"Monitor Status: {monitorStatus.status}");
                    System.Threading.Thread.Sleep(1000);
                }
                while (monitorStatus != null && monitorStatus.status != "ready");

                var companyInfo = await glassdoorWebscraper.DownloadData(snapshotResponse.snapshot_id);

                if (companyInfo.Count > 0)
                {
                    System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(companyInfo,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    var structuredInfo = await GetStructuredInfo(companyInfo[0]);
                    return structuredInfo;
                }
                else
                    System.Console.WriteLine("No information from Glassdoor is available");
            }
            catch (Exception ex)
            {
                Log.Error("Exception occured in ProcessGlassdoorRequest. {Ex}", ex);
            }

            return string.Empty;
        }
    }
}
