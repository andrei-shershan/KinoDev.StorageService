using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs.Models;
using KinoDev.Shared.DtoModels.Orders;
using KinoDev.Shared.DtoModels.Tickets;
using KinoDev.Shared.Helpers;
using KinoDev.Shared.Services;
using KinoDev.Shared.Services.Abstractions;
using KinoDev.StorageService.WebApi.Models.Configurations;
using KinoDev.StorageService.WebApi.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace KinoDev.StorageService.WebApi.Models.Services
{
    public class FileService : IFileService
    {
        private readonly IQRCodeService _qrCodeService;
        private readonly IPdfService _pdfService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<FileService> _logger;
        private readonly IMessageBrokerService _messageBrokerService;

        private readonly DataSettings _dataSettings;

        private readonly BlobStorageSettings _blobStorageSettings;

        private readonly MessageBrokerSettings _messageBrokerSettings;

        public FileService(
            IQRCodeService qrCodeService,
            IPdfService pdfService,
            IBlobStorageService blobStorageService,
            ILogger<FileService> logger,
            IMessageBrokerService messageBrokerService,
            IOptions<MessageBrokerSettings> messageBrokerSettings,
            IOptions<DataSettings> dataSettings,
            IOptions<BlobStorageSettings> blobStorageSettings)
        {
            _qrCodeService = qrCodeService;
            _pdfService = pdfService;
            _blobStorageService = blobStorageService;
            _logger = logger;
            _messageBrokerService = messageBrokerService;
            _messageBrokerSettings = messageBrokerSettings?.Value ?? throw new ArgumentNullException(nameof(messageBrokerSettings));
            _dataSettings = dataSettings?.Value ?? throw new ArgumentNullException(nameof(dataSettings));
            _blobStorageSettings = blobStorageSettings?.Value ?? throw new ArgumentNullException(nameof(blobStorageSettings));
        }

        public async Task GenerateAndUploadFileAsync(OrderSummary orderSummary, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Generating and uploading file for order {OrderId}", orderSummary.Id);

                var qrCode = _qrCodeService.GenerateQRCodeInBase64Async(_dataSettings.QRCodeLink);

                var html = GetHtml(orderSummary, qrCode);

                var pdf = await _pdfService.GeneratePdfAsync(html);

                var hash = HashHelper.CalculateSha256Hash(orderSummary.Id.ToString(), string.Empty);
                var fileName = $"{orderSummary.Id.ToString()}.pdf";

                _logger.LogInformation("Uploading file {FileName} for order {OrderId}", fileName, orderSummary.Id);

                _logger.LogInformation("*** pdf size: {PdfSize} bytes", pdf.Length);
                _logger.LogInformation("*** file name: {FileName}, containerName: {container}", fileName, _blobStorageSettings.ContainerNames.Tickets);

                var relativePath = await _blobStorageService.Upload(pdf, fileName, _blobStorageSettings.ContainerNames.Tickets, PublicAccessType.Blob);

                _logger.LogInformation("File {FileName} uploaded to {RelativePath} for order {OrderId}", fileName, relativePath, orderSummary.Id);

                _logger.LogInformation("File {FileName} uploaded successfully for order {OrderId}", fileName, orderSummary.Id);
                orderSummary.FileUrl = relativePath;

                await _messageBrokerService.SendMessageAsync(
                    _messageBrokerSettings.Queues.OrderFileCreated,
                    orderSummary
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating and uploading file for order {OrderId}", orderSummary.Id);
                throw;
            }    
        }

        public Task<string> UploadPublicFileAsync(byte[] bytes, string fileName, string containerName)
        {
            return _blobStorageService.Upload(bytes, fileName, containerName, PublicAccessType.Blob);
        }

        private string GetHtml(OrderSummary orderSummary, string qrCodeStringContent)
        {
            var html = $@"
                <html>
                <head>
                    <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .header {{ text-align: left; margin-bottom: 30px; }}
                    </style>
                </head>
                <body>
                    <div class=""header"">
                    <h1>{orderSummary.ShowTimeSummary.Movie.Name}</h1>
                    <h3>{orderSummary.ShowTimeSummary.Hall.Name}</h3>
                    <h2>{orderSummary.ShowTimeSummary.Time.ToString("MMMM d, yyyy, HH:mm")}</h2>                    
                    </div>
                    <ul>
                    {GetSeatsList(orderSummary.Tickets)}
                    </ul>
                    <br />
                    <img src=""data:image/png;base64,{qrCodeStringContent}"" alt=""QR Code"" width=""250"" />
                </body>
                </html>";

            return html;
        }

        private string GetSeatsList(IEnumerable<TickerSummary> tickets)
        {
            var sb = new StringBuilder();
            foreach (var ticket in tickets.OrderBy(t => t.Row).ThenBy(t => t.Number))
            {
                sb.AppendLine($"<li>Row: {ticket.Row}, Seat: {ticket.Number}</li>");
            }

            return sb.ToString();
        }
    }
}