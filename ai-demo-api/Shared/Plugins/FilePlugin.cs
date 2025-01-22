using Microsoft.SemanticKernel;
using Shared.Services.FileService;
using System.ComponentModel;

namespace Shared.Plugins;

public class FilePlugin(IFileHandler _fileHandler) : IPlugin
{
    [KernelFunction("list_files")]
    [Description("Returns a list of files stored in the specified directory (relative path).")]
    public async Task<string> ListFiles(
        [Description("The relative directory path to list files from.")] string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return "Invalid directory path. No input provided.";
        }

        var files = await _fileHandler.ListFilesAsync(directoryPath);
        return (files != null && files.Any())
            ? string.Join(", ", files)
            : $"No files found in the directory '{directoryPath}'.";
    }

    [KernelFunction("create_file")]
    [Description("Creates a new file with the given content (relative path).")]
    public async Task<string> CreateFile(
        [Description("The relative path (including filename) for the new file.")] string filePath,
        [Description("The text content of the file.")] string content)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return "Invalid file path. No input provided.";
        }

        await _fileHandler.CreateFileAsync(filePath, content);
        return $"File '{filePath}' created successfully.";
    }

    [KernelFunction("get_file_content")]
    [Description("Retrieves and returns the content of the specified file (relative path).")]
    public async Task<string> GetFileContent(
        [Description("The relative path of the file to read from.")] string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return "Invalid file path. No input provided.";
        }

        var content = await _fileHandler.GetFileContentAsync(filePath);
        return content ?? $"File '{filePath}' is empty or could not be read.";
    }

    [KernelFunction("delete_file")]
    [Description("Deletes the specified file (relative path).")]
    public async Task<string> DeleteFile(
        [Description("The relative path of the file to delete.")] string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return "Invalid file path. No input provided.";
        }

        await _fileHandler.DeleteFileAsync(filePath);
        return $"File '{filePath}' deleted successfully.";
    }

    [KernelFunction("file_exists")]
    [Description("Checks if the specified file exists (relative path).")]
    public async Task<string> FileExists(
        [Description("The relative path of the file to check.")] string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return "Invalid file path. No input provided.";
        }

        bool exists = await _fileHandler.FileExistsAsync(filePath);
        return exists
            ? $"File '{filePath}' exists."
            : $"File '{filePath}' does not exist.";
    }
}

