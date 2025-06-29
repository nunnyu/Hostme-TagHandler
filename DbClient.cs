﻿using System;
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
    public DbClient(IConfiguration config)
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

    public Dictionary<string, int> GetCustomerTagsById(int id)
    {
        var tagDict = new Dictionary<string, int>();

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        string sql = @"
        SELECT td.tag_name, ct.tag_count
        FROM customer_tags ct
        JOIN tag_definitions td ON ct.tag_definition_id = td.id
        WHERE ct.customer_id = @customerId;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@customerId", id);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            string tagName = reader.GetString(0);
            int tagCount = reader.GetInt32(1);

            tagDict[tagName] = tagCount;
        }

        return tagDict;
    }

    public string FormatTagDictionary(Dictionary<string, int> tagDict)
    {
        if (tagDict == null || tagDict.Count == 0)
            return "(No tags found!)";

        var sb = new StringBuilder();
        sb.AppendLine("Customer Tag Summary:");

        foreach (var kvp in tagDict.OrderByDescending(kvp => kvp.Value))
        {
            sb.AppendLine($"- {kvp.Key}: {kvp.Value}");
        }

        return sb.ToString();
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

    public void MetricById(GuestProfile profile, int customerId)
    {
        var paidVisits = profile.OrderCount;
        var totalSpent = profile.TotalOrderPrice;
        string sql = "INSERT INTO customer_statistics (customer_id, paid_visits, total_spent) " +
            "VALUES (@Id, @Visits, @Spent)" +
            "ON CONFLICT (customer_id)" +
            "DO UPDATE SET" +
            "   paid_visits = customer_statistics.paid_visits + EXCLUDED.paid_visits, " +
            "   total_spent = customer_statistics.total_spent + EXCLUDED.total_spent;"; // just add them if the guest is already in the database 

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("Id", customerId);
        cmd.Parameters.AddWithValue("Visits", paidVisits);
        cmd.Parameters.AddWithValue("Spent", totalSpent);

        cmd.ExecuteNonQuery();

        Console.WriteLine($"Added statistics for customer #{customerId}\n");
    }

    public void TagById(int customerId, Dictionary<int, int> tagIdDict)
    {
        if (tagIdDict == null || tagIdDict.Count == 0) return;

        string sql = "INSERT INTO customer_tags (customer_id, tag_definition_id, tag_count) VALUES ";
        var values = new List<string>();

        for (int i = 0; i < tagIdDict.Count; i++)
        {
            values.Add($"(@cus_id, @tag_id{i}, @count{i})");
        }

        sql += string.Join(", ", values);

        sql += @"
            ON CONFLICT (customer_id, tag_definition_id)
            DO UPDATE SET tag_count = customer_tags.tag_count + EXCLUDED.tag_count;";

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);

        // Add shared parameter
        cmd.Parameters.AddWithValue("cus_id", customerId);

        // Add dynamic tag_id parameters
        int c = 0;
        foreach (var kvp in tagIdDict)
        {
            cmd.Parameters.AddWithValue($"tag_id{c}", kvp.Key);
            cmd.Parameters.AddWithValue($"count{c}", kvp.Value);
            c++;
        }

        cmd.ExecuteNonQuery();
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