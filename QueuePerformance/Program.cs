using Azure.Storage.Queues;
using System.Diagnostics;

string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

// Create a unique name for the queue
string queueName = "quickstartqueues-" + Guid.NewGuid().ToString();

// Instantiate a QueueClient to create and interact with the queue
QueueClient queueClient = new QueueClient(connectionString, queueName);

Console.WriteLine($"Creating queue: {queueName}");

// Create the queue
await queueClient.CreateAsync();

var count = 10000;
var tasks = new Task[count];

Console.WriteLine($"Inserting {count} messages...");

var stopWatch = new Stopwatch();
stopWatch.Start();
for (int i = 0; i < count; i++)
{
    var task = queueClient.SendMessageAsync(i.ToString());
    tasks[i] = task;
}
await Task.WhenAll(tasks);
stopWatch.Stop();

var rate = count / stopWatch.Elapsed.TotalSeconds;
Console.WriteLine($"{stopWatch.Elapsed.TotalSeconds} seconds for {count} insertions.\n{rate} insertions per second.");

Console.WriteLine($"Deleting queue: {queueClient.Name}");

await queueClient.DeleteAsync();
