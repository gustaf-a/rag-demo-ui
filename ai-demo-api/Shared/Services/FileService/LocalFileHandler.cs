using Microsoft.Extensions.Configuration;

namespace Shared.Services.FileService;

public class LocalFileHandler(string _rootPath) : IFileHandler
{
    private string EnsureAbsolutePath(string path)
    {
        return Path.IsPathRooted(path)
            ? path
            : Path.Combine(_rootPath, path);
    }

    public async Task CreateFileAsync(string filePath, string content)
    {
        filePath = EnsureAbsolutePath(filePath);

        var directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, content);
    }

    public async Task<string> GetFileContentAsync(string filePath)
    {
        filePath = EnsureAbsolutePath(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        return await File.ReadAllTextAsync(filePath);
    }

    public Task DeleteFileAsync(string filePath)
    {
        filePath = EnsureAbsolutePath(filePath);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> ListFilesAsync(string directoryPath)
    {
        directoryPath = EnsureAbsolutePath(directoryPath);

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var files = Directory.GetFiles(directoryPath).AsEnumerable();
        return Task.FromResult(files);
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        filePath = EnsureAbsolutePath(filePath);

        return Task.FromResult(File.Exists(filePath));
    }
}
