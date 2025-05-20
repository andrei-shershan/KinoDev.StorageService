using System.Text.Json;
using KinoDev.Shared.DtoModels.Orders;
using KinoDev.Shared.Services;
using KinoDev.StorageService.WebApi.Models.Configurations;
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
            return _messageBrokerService.SubscribeAsync(
                    _messageBrokerSettings.Topics.OrderCompleted,
                    _messageBrokerSettings.Queues.OrderCompleted,
                    async (message) =>
                    {
                        var orderSummary = JsonSerializer.Deserialize<OrderSummary>(message);
                        if (orderSummary != null)
                        {
                            await _fileService.GenerateAndUploadFileAsync(orderSummary, stoppingToken);

                        }
                        else
                        {
                            _logger.LogError("Failed to deserialize order summary from message: {Message}", message);
                        }
                    }
                );
        }
    }
}