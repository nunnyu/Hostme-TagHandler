using Azure.Messaging.ServiceBus;
using System;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace HostmeTagHandler;
public class Receiver
{
    private string connectionString;
    private string queueName;
    private static int messagesToView = 3;

    public Receiver(IConfiguration config)
    {
        this.connectionString = config.GetConnectionString("ServiceBusIn") ?? throw new ArgumentNullException("Connection string is missing.");
        this.queueName = config["ServiceBusSettings:QueueName"] ?? throw new ArgumentNullException("Connection string is missing.");
    }

    // Receive messages
    public async Task StartReceivingMessagesAsync()
    {
        await using var client = new ServiceBusClient(connectionString);

        Console.WriteLine("Awaiting session...\n");
        
        var receiver = await client.AcceptNextSessionAsync(queueName);

        // Peek or Receive
        Console.WriteLine("Waiting for messages...\n");
        var messages = await receiver.PeekMessagesAsync(messagesToView);

        if (messages.Count == 0)
        {
            Console.WriteLine("No messages found.");
        }
        else
        {
            int msgCount = 1;
            foreach (var message in messages)
            {
                Console.WriteLine("-- Message " + msgCount + " --");
                msgCount++;

                string body = message.Body.ToString();
                var json = JsonNode.Parse(body);
                var bookingId = json?["BookingId"]?.ToString();

                // Console output
                Console.WriteLine("Message ID: " + message.MessageId);
                Console.WriteLine("Booking ID: " + bookingId);

                if (bookingId != null)
                {
                    await this.ReceiveReceiptInfo(bookingId);
                }
            }
        }
    }

    // Receive receipt info
    private async Task ReceiveReceiptInfo(string bookingId)
    {
        string receiptUrl = $"https://hostmeprod.blob.core.windows.net/bookingsnapshots/{bookingId}/receipt.json";

        using var httpClient = new HttpClient();

        try
        {
            string jsonString = await httpClient.GetStringAsync(receiptUrl);
            JsonNode? json = JsonNode.Parse(jsonString);

            Console.WriteLine($"Receipt Info for Booking ID {bookingId}:\n");
            // Console.WriteLine(json + "\n");

            // Retrieve items and turn them into receipt info
            var itemsJson = json?["items"]?.AsArray();
            List<Item> itemList = new List<Item>();

            if (itemsJson != null)
            {
                foreach (var item in itemsJson)
                {
                    string productId = item?["prodId"]?.ToString() ?? "Unknown";
                    string productName = item?["productName"]?.ToString() ?? "Unknown";
                    int amount = item?["amount"]?.GetValue<int>() ?? 0;
                    double totalPrice = item?["totalPrice"]?.GetValue<double>() ?? 0.0;
                    int course = item?["course"]?.GetValue<int>() ?? 0;
                    DateTime time = item?["creationDate"]?.GetValue<DateTime>() ?? new DateTime(2024, 1, 1, 12, 0, 0);

                    itemList.Add(new Item(productId, productName, amount, totalPrice, course, time));
                }

                // Print all items in the list
                foreach(var item in itemList)
                {
                    Console.WriteLine(item);
                }
            }

        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Failed to fetch receipt for {bookingId}: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error parsing receipt JSON for {bookingId}: {e.Message}");
        }
    }
}