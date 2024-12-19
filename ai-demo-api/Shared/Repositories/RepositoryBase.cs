using Microsoft.Extensions.Configuration;
using Npgsql;
using AiDemos.Api.Configuration;
using AiDemos.Api.Models;
using Shared.Extensions;
using System.Text.Json;

namespace AiDemos.Api.Repositories;

public abstract class RepositoryBase
{
    protected readonly PostgreSqlOptions _options;

    public RepositoryBase(IConfiguration configuration)
    {
        _options = configuration.GetSection(PostgreSqlOptions.PostgreSql).Get<PostgreSqlOptions>() ?? throw new ArgumentNullException(nameof(PostgreSqlOptions));
    }

    public async Task<bool> DoesTableExist(DatabaseOptions databaseOptions)
    {
        var tableNames = await GetTableNames();

        return tableNames.Contains(databaseOptions.TableName);
    }

    public async Task<IEnumerable<string>> GetTableNames()
    {
        string query = @"
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_type = 'BASE TABLE';
        ";

        var parameters = new Dictionary<string, object>();

        return await ExecuteQueryAsync<string>(query, parameters);
    }

    public async Task DeleteTable(DatabaseOptions databaseOptions)
    {
        string dropTableQuery = $"DROP TABLE IF EXISTS {databaseOptions.TableName};";

        await ExecuteQuery(dropTableQuery);
    }

    protected async Task ExecuteQuery(string query)
    {
        using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(query, connection);
        var affected = await command.ExecuteNonQueryAsync();
    }

    #region Helper Methods

    protected async Task ExecuteQueryAsync(string query, Dictionary<string, object> parameters)
    {
        using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(query, connection);

        AddParameters(command, parameters);

        await command.ExecuteNonQueryAsync();
    }

    protected async Task ExecuteQueryAsync(string query, Dictionary<string, object> parameters, Dictionary<string, string> jsonBParameters)
    {
        using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(query, connection);

        AddParameters(command, parameters);

        AddJsonBParameters(command, jsonBParameters);

        await command.ExecuteNonQueryAsync();
    }

    protected async Task<T> ExecuteQuerySingleAsync<T>(string query, Dictionary<string, object> parameters) where T : new()
    {
        using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(query, connection);

        AddParameters(command, parameters);

        using (var reader = await command.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                return MapReaderToEntity<T>(reader);
            }
        }

        return default;
    }

    protected async Task<List<T>> ExecuteQueryAsync<T>(string query, Dictionary<string, object> parameters)
    {
        var results = new List<T>();

        using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(query, connection);

        AddParameters(command, parameters);

        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                T entity = MapReaderToEntity<T>(reader);
                results.Add(entity);
            }
        }

        return results;
    }

    protected async Task<List<T>> ExecuteQueryAsync<T>(string query, Dictionary<string, object> parameters, Dictionary<string, string> jsonBParameters)
    {
        var results = new List<T>();

        using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(query, connection);

        AddParameters(command, parameters);

        AddJsonBParameters(command, jsonBParameters);

        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                T entity = MapReaderToEntity<T>(reader);
                results.Add(entity);
            }
        }

        return results;
    }

    protected async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string query, Dictionary<string, object> parameters, Func<NpgsqlDataReader, T> mapFunction)
    {
        var results = new List<T>();

        using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(query, connection);

        foreach (var parameter in parameters)
            command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(mapFunction(reader));
        }

        return results;
    }

    private static void AddParameters(NpgsqlCommand command, Dictionary<string, object> parameters)
    {
        foreach (var param in parameters)
        {
            var value = param.Value ?? DBNull.Value;
            command.Parameters.AddWithValue(param.Key, value);
        }
    }

    private static void AddJsonBParameters(NpgsqlCommand command, Dictionary<string, string> parameters)
    {
        foreach (var param in parameters)
        {
            command.Parameters.Add(param.Key, NpgsqlTypes.NpgsqlDbType.Jsonb)
                .Value = param.Value;
        }
    }

    private T MapReaderToEntity<T>(NpgsqlDataReader reader)
    {
        Type type = typeof(T);

        if (type == typeof(string) || type.IsPrimitive || type.IsValueType)
        {
            // Handle simple types
            object value = reader.IsDBNull(0) ? default(T) : reader.GetValue(0);
            return (T)Convert.ChangeType(value, typeof(T));
        }
        else
        {
            // Handle complex types
            var obj = Activator.CreateInstance<T>();
            var properties = type.GetProperties();

            foreach (var prop in properties)
            {
                if (!reader.HasColumn(prop.Name))
                    continue;

                var columnValue = reader[prop.Name];

                if (columnValue == DBNull.Value)
                {
                    prop.SetValue(obj, null);
                }
                else
                {
                    prop.SetValue(obj, ConvertValue(columnValue, prop.PropertyType));
                }
            }

            return obj;
        }
    }

    private object ConvertValue(object value, Type targetType)
    {
        if (targetType == typeof(Guid))
            return Guid.Parse(value.ToString());
        if (targetType == typeof(Guid?))
            return value != null ? (Guid?)Guid.Parse(value.ToString()) : null;
        if (targetType == typeof(DateTime))
            return DateTime.Parse(value.ToString());
        if (targetType == typeof(DateTime?))
            return value != null ? (DateTime?)DateTime.Parse(value.ToString()) : null;
        if (targetType == typeof(string))
            return value.ToString();
        if (targetType == typeof(List<Guid>))
            return ((Guid[])value).ToList();
        if (targetType.IsEnum)
            return Enum.Parse(targetType, value.ToString());
        if (targetType == typeof(object))
            return JsonSerializer.Deserialize<object>(value.ToString());

        return Convert.ChangeType(value, targetType);
    }

    #endregion
}
