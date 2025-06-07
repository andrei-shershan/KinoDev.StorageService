namespace KinoDev.StorageService.WebApi.Models.Configurations
{
    public class MessageBrokerSettings
    {
        public Queues Queues { get; set; }
    }

    public class Queues
    {
        public string OrderCompleted { get; set; }

        public string OrderFileCreated { get; set; }
    }
}