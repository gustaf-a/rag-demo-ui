using Azure.Storage.Queues;
using Shared.Configuration;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Shared.Services;

public class ComputerUseService : IComputerUseService
{
    private readonly AzureOptions _azureOptions;

    public ComputerUseService(IConfiguration configuration)
    {
        _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>()
            ?? throw new ArgumentNullException(nameof(AzureOptions));
    }

    public async Task<string> SendTask(string taskInstructions)
    {
        var queueClient = new QueueClient(_azureOptions.ComputerUseQueueStartTask.BlobConnectionString, _azureOptions.ComputerUseQueueStartTask.QueueName);

        // Check if the queue exists
        if (!await queueClient.ExistsAsync())
        {
            return $"Queue '{_azureOptions.ComputerUseQueueStartTask.QueueName}' does not exist.";
        }

        // Create the JSON message
        var messageContent = new
        {
            command = taskInstructions
        };

        string messageJson;
        try
        {
            messageJson = JsonSerializer.Serialize(messageContent);
        }
        catch (JsonException ex)
        {
            return $"Failed to serialize task instructions to JSON: {ex.Message}";
        }

        //var encodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(messageJson));

        try
        {
            // Send the message to the queue
            await queueClient.SendMessageAsync(messageJson);

            return "Successfully sent task to the computer.";
        }
        catch (Exception ex)
        {
            return $"Failed to send the task to the queue: {ex.Message}";
        }
    }
}
