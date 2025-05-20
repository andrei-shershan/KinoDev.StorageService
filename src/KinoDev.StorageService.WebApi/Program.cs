using KinoDev.Shared.Models;
using KinoDev.Shared.Services;
using KinoDev.StorageService.WebApi.Models.Configurations;
using KinoDev.StorageService.WebApi.Models.Services;
using KinoDev.StorageService.WebApi.Services;

namespace KinoDev.StorageService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var blobStorageSettings = builder.Configuration.GetSection("BlobStorage");
            if (blobStorageSettings == null)
            {
                throw new ArgumentNullException(nameof(blobStorageSettings), "BlobStorage settings not found in configuration.");
            }

            var pdfServiceSettings = builder.Configuration.GetSection("PdfService");
            if (pdfServiceSettings == null)
            {
                throw new ArgumentNullException(nameof(pdfServiceSettings), "PdfService settings not found in configuration.");
            }

            var messageBrokerSettings = builder.Configuration.GetSection("MessageBroker");
            if (messageBrokerSettings == null)
            {
                throw new ArgumentNullException(nameof(messageBrokerSettings), "MessageBroker settings not found in configuration.");
            }

            var rabbitmqSettings = builder.Configuration.GetSection("RabbitMQ");
            if (rabbitmqSettings == null)
            {
                throw new ArgumentNullException(nameof(rabbitmqSettings), "RabbitMQ settings not found in configuration.");
            }

            var dataSettings = builder.Configuration.GetSection("Data");
            if (dataSettings == null)
            {
                throw new ArgumentNullException(nameof(dataSettings), "Data settings not found in configuration.");
            }

            builder.Services.Configure<BlobStorageSettings>(blobStorageSettings);
            builder.Services.Configure<PdfServiceSettings>(pdfServiceSettings);
            builder.Services.Configure<MessageBrokerSettings>(messageBrokerSettings);
            builder.Services.Configure<RabbitMqSettings>(rabbitmqSettings);
            builder.Services.Configure<DataSettings>(dataSettings);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddTransient<IQRCodeService, QRCodeService>();
            builder.Services.AddTransient<IPdfService, PdfService>();
            builder.Services.AddTransient<IBlobStorageService, BlobStorageService>();
            builder.Services.AddTransient<IFileService, FileService>();
            builder.Services.AddTransient<IMessageBrokerService, RabbitMQService>();

            builder.Services.AddHostedService<MessagingSubscriber>();

            // Register and configure HttpClient for PdfService
            builder.Services.AddHttpClient<PdfService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
