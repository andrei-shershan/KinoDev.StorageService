namespace KinoDev.StorageService.WebApi.Models.Configurations
{
    public class BlobStorageSettings
    {
        public string ConnectionString { get; set; }

        public ContainerNames ContainerNames { get; set; }
    }

    public class ContainerNames
    {
        public string Tickets { get; set; }

        public string PublicImages { get; set; }
    }
}
