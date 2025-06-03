using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HostmeTagHandler;
using Microsoft.Extensions.Configuration;
using Npgsql;

class DbClient
{
    string connectionString;
    public DbClient (IConfiguration config)
    {
        connectionString = config.GetConnectionString("Database") ?? throw new ArgumentNullException("Database connection string is missing.");
    }

    public void SendSqlCommand(string sql)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.ExecuteNonQuery();
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

    // Creates customer by email, and returns their id 
    public void AddCustomerByEmail(string email)
    {
        string sql = "INSERT INTO customers (email) VALUES (@Email);";

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("Email", email);

        cmd.ExecuteNonQuery();

        Console.WriteLine("Added " + email + " to the database.");
    }

    public bool EmailExists(string email)
    {
        string sql = "SELECT COUNT(*) FROM customers WHERE email = @Email;";
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("Email", email);

        long count = (long)(cmd.ExecuteScalar() ?? 0); // if null it's not there, so 0 
        return count > 0;
    }

    public void ProfileToDb(GuestProfile profile)
    {
        AddCustomerByEmail(profile.identifier);
    }

    public int GetGuestIdByEmail(string email)
    {
        string sql = "SELECT id FROM customers WHERE email = @Email;";

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("Email", email);

        var result = cmd.ExecuteScalar();

        if (result != null && result is int)
        {
            int id = (int)result;
            return id;
        }

        throw new Exception($"No guest found with email: {email}");
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