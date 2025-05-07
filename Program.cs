using Azure.Messaging.ServiceBus;
using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;

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
    }
}