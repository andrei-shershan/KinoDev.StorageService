using System.Text.Json;
using KinoDev.Shared.DtoModels.Orders;
using KinoDev.Shared.Services;
using KinoDev.Shared.Services.Abstractions;
using KinoDev.StorageService.WebApi.Models.Configurations;
using KinoDev.StorageService.WebApi.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace KinoDev.StorageService.WebApi.Services
{
    public class MessagingSubscriber : BackgroundService
    {
        private readonly IMessageBrokerService _messageBrokerService;

        private readonly IFileService _fileService;

        private readonly MessageBrokerSettings _messageBrokerSettings;

        private readonly ILogger<MessagingSubscriber> _logger;

        public MessagingSubscriber(
            IFileService fileService,
            IMessageBrokerService messageBrokerService,
            IOptions<MessageBrokerSettings> messageBrokerSettings,
            ILogger<MessagingSubscriber> logger
            )
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _messageBrokerService = messageBrokerService ?? throw new ArgumentNullException(nameof(messageBrokerService));
            _messageBrokerSettings = messageBrokerSettings?.Value ?? throw new ArgumentNullException(nameof(messageBrokerSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _messageBrokerService.SubscribeAsync<OrderSummary>(
                _messageBrokerSettings.Queues.OrderCompleted,
                async (orderSummary) =>
                {
                    _logger.LogInformation(
                        "Received order summary for order ID {OrderId}",
                        orderSummary.Id
                    );
                    await _fileService.GenerateAndUploadFileAsync(orderSummary, stoppingToken);
                }
            );
        }
    }
}