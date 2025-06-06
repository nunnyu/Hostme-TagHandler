using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using static System.Environment;
using System.Text.Json;

namespace HostmeTagHandler;
class OpenAI
{
    private string deploymentName = "gpt-4.1"; // The model is the same as the name
    private string apiKey;
    private Uri endpoint;
    private AzureOpenAIClient azureClient;
    private ChatClient chatClient;
    private IConfiguration config;


    public OpenAI(IConfiguration config)
    {
        this.config = config;
        apiKey = config.GetConnectionString("OpenAIKey") ?? throw new ArgumentNullException("Connection string is missing.");
        endpoint = new Uri(config.GetConnectionString("OpenAIEndpoint") ?? throw new ArgumentNullException("Connection string is missing."));

        azureClient = new(
        endpoint,
        new AzureKeyCredential(apiKey));
        chatClient = azureClient.GetChatClient(deploymentName);
    }

    public string AnalyzeMessageBundle(MessageBundle messageBundle)
    {
        if (messageBundle == null)
        {
            throw new ArgumentNullException(
                nameof(messageBundle));
        }

        DbClient db = new DbClient(config); // get all of the tags from the database, and then store them as a long list of strings 
        var tagDict = db.GetTagDefinitions();

        string tagInfo = "";
        foreach (var tagName in tagDict.Keys)
        {
            tagInfo += tagName + "; ";
        }

        string bundleText = messageBundle.ToString(); // Contains all the receipts 
        string directions = "Respond with a JSON formatted list containing every tag that fits each receipt. " +
            "Create a new list for every receipt, and label each list \"receipt_x\" where x is the current receipt number. " +
            "Ignore nonsensical dates and prices. The tags should represent the receipt as a whole, and not individual items. \n"; // Directions for what to do with our data
        string prompt = tagInfo + "\n" + bundleText + "\n" + directions;

        return GetResponse(prompt);
    }

    public string AnalyzeDatabaseCustomer(Dictionary<string, int> tagDict)
    {
        DbClient db = new DbClient(config);
        string customerTags = db.FormatTagDictionary(tagDict); // Contains all the tag info 
        string directions = "Respond only with a JSON object with 3 keys, primary_habit, secondary_habit, occasional_habit." +
            "These should be a very short, but high level description of this customer based on their tag info regarding their eating habits. "; // Directions for what to do with our data
        string prompt = directions + "\n" + customerTags;

        return GetResponse(prompt);
    }

    public string GetResponse(string prompt)
    {
        try
        {
            Console.WriteLine("Analyzing received info...\n\n\n");

            var requestOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 4096,
                Temperature = 1.0f,
                TopP = 1.0f,
            };

            List<ChatMessage> messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a food categorization assistant. Given receipt items, return accurate tags based only on what is actually mentioned in the receipt. " +
                "Do NOT guess. Only apply tags that clearly match dish names. Respond only in JSON."),
                new UserChatMessage(prompt),
            };

            var response = chatClient.CompleteChat(messages, requestOptions);
            return response.Value.Content[0].Text;

        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

}
