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


    public OpenAI(IConfiguration config)
    {
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

        string tagInfo = new ReceiptInfo(new List<Item>()).getTagCategories(); // This will give the AI tags to organize the data by 
        string bundleText = messageBundle.ToString(); // Contains all the receipts 
        string directions = "Respond with a JSON formatted list containing every tag that fits each receipt. " +
            "Create a new list for every receipt, and label each list \"receipt_x\" where x is the current receipt number. " +
            "Ignore nonsensical dates and prices. \n"; // Directions for what to do with our data
        string prompt = tagInfo + "\n" + bundleText + "\n" + directions;

        return GetResponse(prompt);
    }


    public string GetResponse(string prompt)
    {
        try
        {
            Console.WriteLine("Analyzing received info...\n");

            var requestOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 4096,
                Temperature = 1.0f,
                TopP = 1.0f,
            };

            List<ChatMessage> messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an assistant, respond only in JSON."),
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
