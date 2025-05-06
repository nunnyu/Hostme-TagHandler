using Azure.Messaging.ServiceBus;
using System;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Configuration;

namespace HostmeTagHandler;
public class Receiver
{
    private string connectionString;
    private string queueName;

    public Receiver(IConfiguration config)
    {
        this.connectionString = config.GetConnectionString("ServiceBusIn") ?? throw new ArgumentNullException("Connection string is missing.");
        this.queueName = config["ServiceBusSettings:QueueName"] ?? throw new ArgumentNullException("Connection string is missing.");
    }

    // Act upon the received json 
    private void OnReceive(string jsonMsg)
    {
        // Console.WriteLine(jsonMsg + "\n");
        // Copy from simulated program from our old file
    }

    // Receive messages
    public async Task StartReceivingMessagesAsync()
    {
        await using var client = new ServiceBusClient(connectionString);

        Console.WriteLine("Awaiting session...\n");
        
        var receiver = await client.AcceptNextSessionAsync(queueName);

        // Peek or Receive
        Console.WriteLine("Waiting for messages...\n");
        var messages = await receiver.PeekMessagesAsync(1);

        if (messages.Count == 0)
        {
            Console.WriteLine("No messages found.");
        }
        else
        {
            foreach (var message in messages)
            {
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

            Console.WriteLine($"Receipt Info for Booking ID {bookingId}:");
            Console.WriteLine(json);
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