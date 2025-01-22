namespace Shared.Services.FileService;

public interface IFileHandler
{
    Task CreateFileAsync(string filePath, string content);
    Task<string> GetFileContentAsync(string filePath);
    Task DeleteFileAsync(string filePath);
    Task<IEnumerable<string>> ListFilesAsync(string directoryPath);
    Task<bool> FileExistsAsync(string filePath);
}
