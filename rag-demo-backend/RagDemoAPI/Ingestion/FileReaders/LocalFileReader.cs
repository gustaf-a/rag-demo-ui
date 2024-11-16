using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.FileReaders;

public class LocalFileReader(string _folderPath) : IFileReader
{
    private readonly List<string> _files = Directory.GetFiles(_folderPath).ToList();

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

        var metaData = CreateMetaData(filePath, content);

        return new IngestionSource
        {
            Name = _files[_filesRead],
            Content = content
        };
    }

    public EmbeddingMetaData CreateMetaData(string filePath, string content)
    {
        return new EmbeddingMetaData
        {
            CreatedDateTime = DateTime.UtcNow,
            Source = _folderPath,
            Uri = filePath
        };
    }
}
