namespace RagDemoAPI.Services;

public interface IPostgreSqlService
{
    Task ResetDatabaseAsync();
    Task SetupTablesAsync();
    Task InsertDataAsync(string content, float[] embedding);
}