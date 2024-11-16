using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.FileReaders;

public class AzureBlobFileReader : IFileReader
{
    private readonly List<string> _blobs = [];
    private readonly BlobContainerClient _containerClient;
    private int _blobsRead = -1;

    public AzureBlobFileReader(IngestFromAzureContainerOptions azureContainerOptions)
    {
        if (string.IsNullOrWhiteSpace(azureContainerOptions.ConnectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(azureContainerOptions.ConnectionString));

        if (string.IsNullOrWhiteSpace(azureContainerOptions.ContainerName))
            throw new ArgumentException("Container name cannot be null or empty.", nameof(azureContainerOptions.ContainerName));

        _containerClient = new BlobContainerClient(azureContainerOptions.ConnectionString, azureContainerOptions.ContainerName);
        InitializeBlobsAsync(azureContainerOptions).GetAwaiter().GetResult();
    }

    private async Task InitializeBlobsAsync(IngestFromAzureContainerOptions azureContainerOptions)
    {
        try
        {
            await _containerClient.CreateIfNotExistsAsync();

            await foreach (BlobItem blob in _containerClient.GetBlobsAsync())
            {
                if (!blob.Deleted)
                {
                    _blobs.Add(blob.Name);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize blobs from Azure Blob Storage.", ex);
        }
    }

    public Task<int> GetFileCount()
    {
        return Task.FromResult(_blobs.Count);
    }

    public async Task<IngestionSource> GetNextFileContent()
    {
        _blobsRead++;

        if (_blobsRead >= _blobs.Count)
            return null;

        string blobName = _blobs[_blobsRead];
        BlobClient blobClient = _containerClient.GetBlobClient(blobName);

        try
        {
            var downloadResult = await blobClient.DownloadContentAsync();

            var metaData = CreateMetaData(blobName, downloadResult);

            return new IngestionSource
            {
                Content = downloadResult.Value.Content.ToString(),
                MetaData = metaData,
                Name = blobName
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read blob content for '{blobName}'.", ex);
        }
    }

    private EmbeddingMetaData CreateMetaData(string blobName, Response<BlobDownloadResult> downloadResult)
    {
        return new EmbeddingMetaData
        {
            Source = _containerClient.Uri.AbsoluteUri,
            CreatedDateTime = DateTime.UtcNow,
            Uri = $"{_containerClient.Uri.AbsoluteUri}/{blobName}"
        };
    }
}
