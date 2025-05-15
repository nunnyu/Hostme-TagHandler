using Azure.Messaging.ServiceBus;
using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Azure.AI.Translation.Text;

namespace HostmeTagHandler;
class Program
{
    // This is just for testing purposes, this can be removed later
    private static Boolean receivingData = true;

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

        if (receivingData)
        {
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
        }
        else
        {
            // This has sample receipt data in French
            string testPrompt = "MeatLover, \r\nSoupFan, \r\nSteakFan,\r\nSeafoodFan,\r\nPlantBased,\r\nGlutenFree,\r\nLowCarb, \r\nComfortFood, \r\nPastaFan,\r\nSpicyFoodFan,\r\nSweetsFan,\r\nLuxuryFood, \r\nWhiteWineFan,\r\nRedWineFan,\r\nWhiskeyFan,\r\nBeerFan,\r\nNonAlcoholicPreference,\r\nCocktailFan,\r\nEarlyBird,\r\nLunch,\r\nDinner,\r\nLateNight,\r\nWeekend,\r\n\r\n" +
                "-- Receipt Info: 1 --\r\nName: Croissant au beurre artisanal  \r\nPrice: 2.80  \r\nOrder Date: 4/15/2025 9:13:47 AM  \r\n\r\nName: Café crème  \r\nPrice: 3.20  \r\nOrder Date: 4/15/2025 9:15:02 AM  \r\n\r\nName: Jus d'orange pressé  \r\nPrice: 4.00  \r\nOrder Date: 4/15/2025 9:16:30 AM  \r\n\r\n\r\n-- Receipt Info: 2 --\r\nName: Soupe à l'oignon gratinée  \r\nPrice: 6.50  \r\nOrder Date: 4/28/2025 12:30:11 PM  \r\n\r\nName: Quiche lorraine  \r\nPrice: 7.80  \r\nOrder Date: 4/28/2025 12:45:05 PM  \r\n\r\nName: Tarte tatin  \r\nPrice: 5.90  \r\nOrder Date: 4/28/2025 1:10:33 PM  \r\n\r\n\r\n-- Receipt Info: 3 --\r\nName: Assiette de fromages variés  \r\nPrice: 8.90  \r\nOrder Date: 5/2/2025 6:48:22 PM  \r\n\r\nName: Baguette tradition  \r\nPrice: 1.40  \r\nOrder Date: 5/2/2025 6:50:10 PM  \r\n\r\nName: Verre de vin rouge (Merlot)  \r\nPrice: 5.60  \r\nOrder Date: 5/2/2025 6:52:48 PM  \r\n\r\n" +
                "Respond with a JSON formatted list containing every tag that fits each receipt. Create a new list for every receipt, and label each list \"receipt_x\" where x is the current receipt number.";

            Console.WriteLine(testPrompt);

            OpenAI openAI = new OpenAI(config);
            string tagData = openAI.GetResponse(testPrompt);
        }
    }
}