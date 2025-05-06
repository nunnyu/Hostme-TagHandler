using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Configuration;

namespace HostmeTagHandler;
class Sender
{
    private string connectionString;
    private string queueName;

    public Sender(IConfiguration config)
    {
        this.connectionString = config.GetConnectionString("PersonalTesting") ?? throw new ArgumentNullException("Connection string is missing.");
        this.queueName = "practice-messages";
    }
    public async Task StartSendMessageAsync(String msg)
    {
        // New client
        ServiceBusClient client = new ServiceBusClient(connectionString);

        // Create a sender for our queue
        ServiceBusSender sender = client.CreateSender(queueName);

        // Create our little message 
        ServiceBusMessage message = new ServiceBusMessage(msg);

        // Send the message to the Service Bus queue
        await sender.SendMessageAsync(message);

        // Confirmation in the console
        Console.WriteLine("Sent!\n");

        // Dispose the sender and client
        await sender.DisposeAsync();
        await client.DisposeAsync();
    }
}