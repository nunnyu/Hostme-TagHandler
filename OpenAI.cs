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
    private string deploymentName = "o3-mini"; // The model is the same as the name
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

    public async Task<string> AnalyzeMessageBundle(MessageBundle messageBundle)
    {
        if (messageBundle == null)
        {
            throw new ArgumentNullException(
                nameof(messageBundle));
        }

        ReceiptInfo receiptInfo = messageBundle.receiptInfoList[0];

        string tagInfo = receiptInfo.getTags(); // This will give the AI tags to organize the data by 
        string bundleText = messageBundle.ToString(); // Contains all the receipts 
        string directions = "Respond with a JSON formatted list containing every tag that fits each receipt. " +
            "Create a new list for every receipt, and label each list \"receipt_x\" where x is the current receipt number. " +
            "Ignore nonsensical dates and prices. \n";
        string prompt = tagInfo + "\n" + bundleText + "\n" + directions;

        return await GetResponseAsync(prompt);
    }


    public async Task<string> GetResponseAsync(string prompt)
    {
        try
        {
            Console.WriteLine("Analyzing received info...\n");

            var messages = new List<ChatMessage>
            {
                // Send prompt to OpenAI
                new UserChatMessage(@"" + prompt)
            };

            // Create chat completion options
            var options = new ChatCompletionOptions();

            try
            {
                // Create the chat completion request
                ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

                // Print the response
                if (completion != null)
                {
                    string output = JsonSerializer.Serialize(completion, new JsonSerializerOptions() { WriteIndented = true });
                    return output;
                }
                else
                {
                    Console.WriteLine("No response received.\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Completion error: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }

        return "";
    }

}
