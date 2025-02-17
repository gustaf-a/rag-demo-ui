﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Shared.Models;

namespace AiDemos.Api.Ingestion.FileReaders;

public class AzureBlobFileReader : FileReaderBase, IFileReader
{
    private readonly List<string> _blobs = [];
    private readonly BlobContainerClient _containerClient;
    private int _blobsRead = -1;

    public AzureBlobFileReader(IngestDataRequest request) : base(request.MetaDataTags)
    {
        var azureContainerOptions = request.IngestFromAzureContainerOptions;

        if (string.IsNullOrWhiteSpace(azureContainerOptions.ConnectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(azureContainerOptions.ConnectionString));

        if (string.IsNullOrWhiteSpace(azureContainerOptions.RootContainerName))
            throw new ArgumentException("Container name cannot be null or empty.", nameof(azureContainerOptions.RootContainerName));

        _containerClient = new BlobContainerClient(azureContainerOptions.ConnectionString, azureContainerOptions.RootContainerName);
        InitializeBlobsAsync(azureContainerOptions).GetAwaiter().GetResult();
    }

    private async Task InitializeBlobsAsync(IngestFromAzureContainerOptions azureContainerOptions)
    {
        try
        {
            if(!string.IsNullOrWhiteSpace(azureContainerOptions.SubFolderPrefix))
                azureContainerOptions.SubFolderPrefix = azureContainerOptions.SubFolderPrefix.TrimEnd('/') + "/";

            await foreach (BlobItem blob in _containerClient.GetBlobsAsync(prefix: azureContainerOptions.SubFolderPrefix))
            {
                if (!azureContainerOptions.IncludeSubfolders)
                {
                    var directory = Path.GetDirectoryName(blob.Name)?.Replace('\\', '/').TrimEnd('/') + "/";

                    if (!string.Equals(directory, azureContainerOptions.SubFolderPrefix, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                if (!blob.Deleted)
                    _blobs.Add(blob.Name);
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

    public async Task<IngestionSource> GetNextFile(bool includeContent)
    {
        _blobsRead++;

        if (_blobsRead >= _blobs.Count)
            return null;

        string blobName = _blobs[_blobsRead];
        BlobClient blobClient = _containerClient.GetBlobClient(blobName);

        try
        {
            var result = new IngestionSource
            {
                Name = blobName
            };

            if (includeContent)
            {
                var downloadResult = await blobClient.DownloadContentAsync();
                result.Content = downloadResult.Value.Content.ToString();
            }

            result.MetaData = CreateMetaData(_containerClient.Uri.AbsoluteUri, $"{_containerClient.Uri.AbsoluteUri}/{blobName}");

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read blob content for '{blobName}'.", ex);
        }
    }
}
