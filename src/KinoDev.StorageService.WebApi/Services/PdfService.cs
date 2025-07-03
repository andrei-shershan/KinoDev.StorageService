using System.Net.Http.Headers;
using System.Text;
using KinoDev.StorageService.WebApi.Models.Configurations;
using KinoDev.StorageService.WebApi.Services.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace KinoDev.StorageService.WebApi.Services
{
    public class PdfService : IPdfService
    {
        private readonly HttpClient _httpClient;
        private readonly PdfServiceSettings _pdfServiceSettings;

        private readonly ILogger<PdfService> _logger;

        public PdfService(
            HttpClient httpClient,
            IOptions<PdfServiceSettings> options,
            ILogger<PdfService> logger
        )
        {
            _pdfServiceSettings = options.Value;

            _logger = logger;

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_pdfServiceSettings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/pdf");
        }

        public async Task<byte[]> GeneratePdfAsync(string htmlContent)
        {
            try
            {
                var requestUri = "api/getpdf";
                var requestContent = new StringContent(JsonConvert.SerializeObject(new { html = htmlContent }), Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending request to PDF generation service at {RequestUri}", requestUri);

                var response = await _httpClient.PostAsync(requestUri, requestContent);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                throw new Exception($"PDF generation failed with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating PDF: {ex.Message}", ex);
            }
        }
    }
}