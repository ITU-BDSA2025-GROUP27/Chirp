using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace Chirp.Razor;

public class DBFacade
{
    private readonly string _dbPath;

    public DBFacade(string dbPath)
    {
        _dbPath = dbPath;
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        if (File.Exists(_dbPath))
        {
            return;
        }

        var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

        var schemaFileInfo = embeddedProvider.GetFileInfo("data.schema.sql");
        using var schemaReader = new StreamReader(schemaFileInfo.CreateReadStream());
        var schemaQuery = schemaReader.ReadToEnd();

        var dumpFileInfo = embeddedProvider.GetFileInfo("data.dump.sql");
        using var dumpReader = new StreamReader(dumpFileInfo.CreateReadStream());
        var dumpQuery = dumpReader.ReadToEnd();

        using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
        {
            connection.Open();

            var schemaCommand = connection.CreateCommand();
            schemaCommand.CommandText = schemaQuery;
            schemaCommand.ExecuteNonQuery();

            var dumpCommand = connection.CreateCommand();
            dumpCommand.CommandText = dumpQuery;
            dumpCommand.ExecuteNonQuery();
        }
    }

    public List<(string Author, string Message, long Timestamp)> GetAllCheeps()
    {
        var cheeps = new List<(string, string, long)>();

        var sqlQuery = @"SELECT user.username, message.text, message.pub_date
                        FROM message
                        JOIN user ON message.author_id = user.user_id
                        ORDER BY message.pub_date DESC";

        using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = sqlQuery;

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var author = reader.GetString(0);
                var message = reader.GetString(1);
                var timestamp = reader.GetInt64(2);
                cheeps.Add((author, message, timestamp));
            }
        }

        return cheeps;
    }

    public List<(string Author, string Message, long Timestamp)> GetCheepsByAuthor(string author)
    {
        var cheeps = new List<(string, string, long)>();

        var sqlQuery = @"SELECT user.username, message.text, message.pub_date
                        FROM message
                        JOIN user ON message.author_id = user.user_id
                        WHERE user.username = @author
                        ORDER BY message.pub_date DESC";

        using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = sqlQuery;
            command.Parameters.AddWithValue("@author", author);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var cheepAuthor = reader.GetString(0);
                var message = reader.GetString(1);
                var timestamp = reader.GetInt64(2);
                cheeps.Add((cheepAuthor, message, timestamp));
            }
        }

        return cheeps;
    }
}
