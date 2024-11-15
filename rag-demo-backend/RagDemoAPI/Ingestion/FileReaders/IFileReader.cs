using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.FileReaders
{
    public interface IFileReader
    {
        Task<int> GetFileCount();

        /// <summary>
        /// Asynchronously retrieves the content of the next file in the location..
        /// </summary>
        /// <returns>The content of the next file as a string, or null if no more files are available.</returns>
        Task<IngestionSource> GetNextFileContent();
    }
}