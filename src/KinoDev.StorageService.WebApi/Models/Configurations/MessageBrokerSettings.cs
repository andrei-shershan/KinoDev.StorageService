namespace KinoDev.StorageService.WebApi.Models.Configurations
{
    public class MessageBrokerSettings
    {
        public Topics Topics { get; set; }

        public Queues Queues { get; set; }
    }

    public class Topics
    {
        public string OrderCompleted { get; set; }
        
        public string OrderFileCreated { get; set; }
    }

    public class Queues
    {
        public string OrderCompleted { get; set; }
    }
}