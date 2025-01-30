using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Shared.Configuration;

namespace Shared.Services.FileService;

public class AzureBlobStorageFileHandler : IFileHandler
{
    private readonly AzureOptions _azureOptions;
    private readonly BlobContainerClient _blobContainerClient;

    public AzureBlobStorageFileHandler(IConfiguration configuration)
    {
        _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>() 
            ?? throw new ArgumentNullException(nameof(AzureOptions));

        ArgumentNullException.ThrowIfNullOrWhiteSpace(nameof(_azureOptions.FileStorageBlob.ConnectionString));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(nameof(_azureOptions.FileStorageBlob.RootContainerName));

        _blobContainerClient = new(_azureOptions.FileStorageBlob.ConnectionString, _azureOptions.FileStorageBlob.RootContainerName);
    }

    public async Task CreateFileAsync(string filePath, string content)
    {
        await _blobContainerClient.CreateIfNotExistsAsync();

        var blobClient = _blobContainerClient.GetBlobClient(filePath);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await blobClient.UploadAsync(stream, overwrite: true);
    }

    public async Task<string> GetFileContentAsync(string filePath)
    {
        var blobClient = _blobContainerClient.GetBlobClient(filePath);

        if (!await blobClient.ExistsAsync())
        {
            throw new FileNotFoundException($"Blob not found: {filePath}");
        }

        var downloadInfo = await blobClient.DownloadContentAsync();
        return downloadInfo.Value.Content.ToString();
    }

    public async Task DeleteFileAsync(string filePath)
    {
        var blobClient = _blobContainerClient.GetBlobClient(filePath);
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string directoryPath)
    {
        var blobs = _blobContainerClient.GetBlobsAsync(prefix: directoryPath);
        var blobNames = new List<string>();

        await foreach (var blobItem in blobs)
        {
            blobNames.Add(blobItem.Name);
        }

        return blobNames;
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        var blobClient = _blobContainerClient.GetBlobClient(filePath);
        return await blobClient.ExistsAsync();
    }
}
