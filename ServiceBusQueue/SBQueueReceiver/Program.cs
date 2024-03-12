// See https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample02_MessageSettlement.md

using Azure.Messaging.ServiceBus;

var connectionString = Environment.GetEnvironmentVariable("AZURE_SBQ_CONNECTION_STRING");
var queueName = "q";
// since ServiceBusClient implements IAsyncDisposable we create it with "await using"
await using var client = new ServiceBusClient(connectionString);

// create the sender
var sender = client.CreateSender(queueName);

// create a message that we can send
var content = "Hello world!";
var message = new ServiceBusMessage(content);

// send the message
Console.WriteLine($"Send: '{content}'");
await sender.SendMessageAsync(message);

// create a receiver that we can use to receive and settle the message
var receiver = client.CreateReceiver(queueName);

// the received message is a different type as it contains some service set properties
var receivedMessage = await receiver.ReceiveMessageAsync();
Console.WriteLine($"Received: '{receivedMessage.Body.ToString()}'");

// If we know that we are going to be processing the message for a long time, we can extend the lock for the message
// by the configured LockDuration (by default, 30 seconds).
await receiver.RenewMessageLockAsync(receivedMessage);

// abandon the message, thereby releasing the lock and allowing it to be received again by this or other receivers
// await receiver.AbandonMessageAsync(receivedMessage);

// complete the message, thereby deleting it from the service
await receiver.CompleteMessageAsync(receivedMessage);

// Receive messages in batch

Console.WriteLine("Try to receive more messages in batch...");
//await sender.SendMessageAsync(new ServiceBusMessage("Hello!"));
//await sender.SendMessageAsync(new ServiceBusMessage("World!"));

var messages = await receiver.ReceiveMessagesAsync(10, TimeSpan.MaxValue);
Console.WriteLine($"Received in batch: {messages.Count} messages.");

var tasks = new Task[messages.Count];
for (var i = 0; i < messages.Count; i++)
{
    Console.WriteLine(messages[i].Body.ToString());
    tasks[i] = receiver.CompleteMessageAsync(messages[i]);
}
await Task.WhenAll(tasks);