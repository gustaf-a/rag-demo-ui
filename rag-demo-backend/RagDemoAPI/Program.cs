
using Microsoft.SemanticKernel;
using RagDemoAPI.Configuration;

namespace RagDemoAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:8080")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            SetupAzure(builder);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowFrontend");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void SetupAzure(WebApplicationBuilder builder)
        {
            var azureSettings = builder.Configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>();

            var kernelBuilder = Kernel.CreateBuilder();
            builder.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Information));

            builder.Services.AddHttpClient();

            builder.Services.AddAzureOpenAIChatCompletion(
                    deploymentName: azureSettings.DeploymentName,
                    endpoint: azureSettings.Endpoint,
                    apiKey: azureSettings.ApiKey);

            builder.Services.AddTransient((serviceProvider) => {
                return new Kernel(serviceProvider);
            });
        }
    }
}
