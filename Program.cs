﻿using Azure.Messaging.ServiceBus;
using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Azure.AI.Translation.Text;
using System.Security.Cryptography;

namespace HostmeTagHandler;
class Program
{
    static async Task Main(string[] args)
    {
        // Allows cyrillic 
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Intro message
        Console.WriteLine("~ AI Marketing Co-Pilot Tagging System Prototype ~\n");

        // Get private keys 
        var config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

        // Receive message 
        var receiver = new Receiver(config);
        await receiver.StartReceivingMessagesAsync();

        // Create a new message bundle to analyze
        MessageBundle messageBundle = new MessageBundle(receiver.receiptInfoList);

        Console.WriteLine(messageBundle);

        // Analyze with Azure OpenAI
        OpenAI openAI = new OpenAI(config);
        string tagDataJSON = openAI.AnalyzeMessageBundle(messageBundle);

        // Create a dictionary with receipt data 
        var dict = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(tagDataJSON);

        List<List<string>> tagLists = new List<List<string>>();
        if (dict != null)
        {
            tagLists = dict.Values.ToList();
        }

        messageBundle.TagReceipts(tagLists);
        messageBundle.PrintTags();

        // Guest Profiles
        List<GuestProfile?> listOfProfiles = messageBundle.BuildProfiles();

        foreach (var profile in listOfProfiles)
        {
            Console.WriteLine(profile);
        }

        Console.WriteLine("Merging profiles...\n");

        List<GuestProfile> profilesToDb = new List<GuestProfile>();

        Console.WriteLine(listOfProfiles.Count + " profiles available.\n");

        foreach (var profile in listOfProfiles)
        {
            if (profile == null)
            {
                continue;
            }

            var unique = true;

            // Check if the list already contains this profile's email in the same session
            foreach (var addedProfile in profilesToDb)
            {
                if (addedProfile.identifier == profile.identifier) // If it has the same identifier, merge them
                {
                    addedProfile.Merge(profile);
                    unique = false;
                }
            }

            if (unique)
            {
                profilesToDb.Add(profile);
            }
        }

        Console.WriteLine(profilesToDb.Count + " profiles being updated in the database.\n");

        Console.WriteLine("\nMerge complete. New profiles below: \n");

        foreach (var profile in profilesToDb)
        {
            Console.WriteLine(profile);
        }

        Console.WriteLine(""); // Just to separate

        var db = new DbClient(config);

        db.Peek();
        var tagDict = db.GetTagDefinitions();

        // Add profiles to the database now
        foreach (var profile in profilesToDb)
        {
            if (!db.EmailExists(profile.identifier))
            {
                db.AddCustomerByEmail(profile.identifier); // Creates a new guest
            }

            int customerId = db.GetGuestIdByEmail(profile.identifier);

            Dictionary<string, int> allTags = profile.TagCounts; // Get the list of tags that we want to add to the database
            Dictionary<int, int> tagIds = new();

            foreach (var tag in allTags)
            {
                if (tagDict.ContainsKey(tag.Key)) // If this tag isn't recognized in the database, we'll just skip it for now 
                {
                    tagIds.Add(tagDict[tag.Key], tag.Value);
                }
            }

            // Apply tagging
            db.TagById(customerId, tagIds);

            // Adding general customer statistics 
            db.MetricById(profile, customerId);
        }

        db.Peek();

        // Example analysis, Henry! His id is 7.
        Console.WriteLine("\n~ Analysis ~\n");
    
        Dictionary<string, int> henryTags = db.GetCustomerTagsById(7);

        Console.WriteLine("(customer_id 7)\n" + db.FormatTagDictionary(henryTags) + "\n");

        Console.WriteLine(openAI.AnalyzeDatabaseCustomer(henryTags));

        // Search Query Test
        string request = "стейк мраморной говядины";
        string output = openAI.SearchQuery(tagDict, request);

        Console.WriteLine($"Search query for {request}: {output}");
    }
}