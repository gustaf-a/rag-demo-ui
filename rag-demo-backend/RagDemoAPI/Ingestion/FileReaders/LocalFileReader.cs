using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.FileReaders;

public class LocalFileReader(string folderPath) : IFileReader
{
    private readonly List<string> _files = Directory.GetFiles(folderPath).ToList();

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

        var content = await File.ReadAllTextAsync(_files[_filesRead]);

        return new IngestionSource
        {
            Name = _files[_filesRead],
            Content = content
        };
    }
}
