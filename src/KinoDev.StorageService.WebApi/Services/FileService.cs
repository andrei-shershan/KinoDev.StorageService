using System.Text;
using KinoDev.Shared.DtoModels.Orders;
using KinoDev.Shared.DtoModels.Tickets;
using KinoDev.Shared.Helpers;
using KinoDev.Shared.Services;
using KinoDev.StorageService.WebApi.Models.Configurations;
using KinoDev.StorageService.WebApi.Services;
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

        private readonly MessageBrokerSettings _messageBrokerSettings;

        public FileService(
            IQRCodeService qrCodeService,
            IPdfService pdfService,
            IBlobStorageService blobStorageService,
            ILogger<FileService> logger,
            IMessageBrokerService messageBrokerService,
            IOptions<MessageBrokerSettings> messageBrokerSettings,
            IOptions<DataSettings> dataSettings)
        {
            _qrCodeService = qrCodeService;
            _pdfService = pdfService;
            _blobStorageService = blobStorageService;
            _logger = logger;
            _messageBrokerService = messageBrokerService;
            _messageBrokerSettings = messageBrokerSettings?.Value ?? throw new ArgumentNullException(nameof(messageBrokerSettings));
            _dataSettings = dataSettings?.Value ?? throw new ArgumentNullException(nameof(dataSettings));
        }

        public async Task GenerateAndUploadFileAsync(OrderSummary orderSummary, CancellationToken cancellationToken)
        {
            var qrCode = _qrCodeService.GenerateQRCodeInBase64Async(_dataSettings.QRCodeLink);

            var html = GetHtml(orderSummary, qrCode);

            var pdf = await _pdfService.GeneratePdfAsync(html);

            var hash = HashHelper.CalculateSha256Hash(orderSummary.Id.ToString(), string.Empty);
            var fileName = $"{hash}.pdf";

            var uri = await _blobStorageService.Upload(pdf, fileName, "test", Azure.Storage.Blobs.Models.PublicAccessType.Blob);
            var relativePath = uri.AbsolutePath.TrimStart('/');

            orderSummary.FileUrl = relativePath;

            await _messageBrokerService.PublishAsync(
                orderSummary,
                _messageBrokerSettings.Topics.OrderFileCreated
                );

            _logger.LogInformation("File uploaded to blob storage: {FileUrl}", uri);
        }

        private string GetHtml(OrderSummary orderSummary, string qrCodeStringContent)
        {
            var getSeatsList = (IEnumerable<TickerSummary> ts) =>
            {
                var sb = new StringBuilder();
                foreach (var ticket in ts)
                {
                    sb.AppendLine($"<li>Row: {ticket.Row}, Seat: {ticket.Number}</li>");
                }

                return sb.ToString();
            };
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
                    {getSeatsList(orderSummary.Tickets)}
                    </ul>
                    <br />
                    <img src=""data:image/png;base64,{qrCodeStringContent}"" alt=""QR Code"" width=""250"" />

                    <br />
                    <br />
                    <small><em>* Do not share this ticket with anyone.</em></small>
                    <br />
                    <small><em>Share this githuib repo with your team :)</em></small>
                </body>
                </html>";

            return html;
        }
    }
}