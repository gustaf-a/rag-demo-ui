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

    public async Task<IngestionSource> GetNextFileContent()
    {
        if(_filesRead >= _files.Count)
            return null;

        _filesRead++;

        var filePath = _files[_filesRead];

        var content = await File.ReadAllTextAsync(filePath);

        var metaData = CreateMetaData(_folderPath, filePath);

        return new IngestionSource
        {
            Name = _files[_filesRead],
            Content = content,
            MetaData = metaData
        };
    }
}
