using Shared.Models;

namespace AiDemos.Api.Ingestion.FileReaders;

public class LocalFileReader(IngestDataRequest request) : FileReaderBase(request.MetaDataTags), IFileReader
{
    private readonly string _folderPath = request.FolderPath;

    private readonly List<string> _files = Directory.GetFiles(request.FolderPath).ToList();

    private int _filesRead = -1;

    public Task<int> GetFileCount()
    {
        return Task.FromResult(_files.Count);
    }

    public async Task<IngestionSource> GetNextFile(bool includeContent)
    {
        if(_filesRead >= _files.Count)
            return null;

        _filesRead++;

        var filePath = _files[_filesRead];

        var result = new IngestionSource
        {
            Name = filePath
        };

        if (includeContent)
            result.Content = await File.ReadAllTextAsync(filePath);

        result.MetaData = CreateMetaData(_folderPath, filePath);

        return result;
    }
}
