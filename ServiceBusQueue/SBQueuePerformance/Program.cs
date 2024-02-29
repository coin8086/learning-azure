using Azure.Messaging.ServiceBus;
using System.Diagnostics;
using System.Text;

string? connectionString = Environment.GetEnvironmentVariable("AZURE_SBQ_CONNECTION_STRING");

var clientOptions = new ServiceBusClientOptions()
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
};
await using var client = new ServiceBusClient(connectionString, clientOptions);
await using var sender = client.CreateSender("q1");

// Get message to insert
string? msgStr = null;
if (args.Length == 1)
{
    if (args[0] == "-")
    {
        var builder = new StringBuilder();
        string? s;
        while ((s = Console.ReadLine()) != null)
        {
            builder.Append(s);
        }
        msgStr = builder.ToString();
    }
    else
    {
        msgStr = args[0];
    }
}
else
{
    msgStr = "abcd";
}

Console.WriteLine($"Message length: {msgStr.Length}");

var msg = new ServiceBusMessage(msgStr);

var count = 10000;
Console.WriteLine($"Inserting {count} messages");

//var messageBatch = await sender.CreateMessageBatchAsync();
var tasks = new List<Task>();
var stopWatch = new Stopwatch();
stopWatch.Start();
for (var i = 0; i < count; i++)
{
    //if (!messageBatch.TryAddMessage(msg) || i == count - 1)
    //{
    //    var task = sender.SendMessagesAsync(messageBatch);
    //    tasks.Add(task);
    //    messageBatch = await sender.CreateMessageBatchAsync();
    //}

    var task = sender.SendMessageAsync(msg);
    tasks.Add(task);
}
await Task.WhenAll(tasks);
stopWatch.Stop();

var rate = count / stopWatch.Elapsed.TotalSeconds;
Console.WriteLine($"{stopWatch.Elapsed.TotalSeconds} seconds for {count} insertions.\n{rate} insertions per second.");
