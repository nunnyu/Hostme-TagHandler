using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

class DbClient
{
    string connectionString;
    public DbClient (IConfiguration config)
    {
        connectionString = config.GetConnectionString("Database") ?? throw new ArgumentNullException("Database connection string is missing.");
    }

    public Dictionary<string, int> GetTagDefinitions()
    {
        var tagDict = new Dictionary<string, int>();

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT id, tag_name FROM tag_definitions;", conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            int tagId = reader.GetInt32(0);
            string tagName = reader.GetString(1);

            tagDict[tagName] = tagId;
        }

        return tagDict;
    }

    // Mostly a test to see if the connection is working
    public void Peek()
    {
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            Console.WriteLine("We've connected");

            using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Customers", conn))
            {
                var count = cmd.ExecuteScalar();
                Console.WriteLine($"There are {count} customers in the database.");
            }
        }
    }
}