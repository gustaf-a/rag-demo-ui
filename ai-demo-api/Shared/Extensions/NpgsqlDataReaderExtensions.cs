namespace Shared.Extensions;

using System;
using Npgsql;

public static class NpgsqlDataReaderExtensions
{
    public static bool HasColumn(this NpgsqlDataReader reader, string columnName)
    {
        try
        {
            return reader.GetOrdinal(columnName) >= 0;
        }
        catch (IndexOutOfRangeException)
        {
            return false;
        }
    }
}
