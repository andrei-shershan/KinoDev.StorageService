using System.Net.Http.Headers;
using System.Text;
using KinoDev.StorageService.WebApi.Models.Configurations;
using KinoDev.StorageService.WebApi.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace KinoDev.StorageService.WebApi.Services
{
    public class PdfService : IPdfService
    {
        private readonly HttpClient _httpClient;
        private readonly PdfServiceSettings _pdfServiceSettings;

        public PdfService(
            HttpClient httpClient,
            IOptions<PdfServiceSettings> options
        )
        {
            _pdfServiceSettings = options.Value;

            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/pdf");
        }

        public async Task<byte[]> GeneratePdfAsync(string htmlContent)
        {
            try
            {
                using var form = new MultipartFormDataContent();
                var stringContent = new StringContent(htmlContent, Encoding.UTF8, "text/html");
                stringContent.Headers.ContentType = new MediaTypeHeaderValue("text/html");

                form.Add(stringContent, "files", "index.html");

                var response = await _httpClient.PostAsync($"{_pdfServiceSettings.BaseUrl}/forms/chromium/convert/html", form);
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