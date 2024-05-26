using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SBQSendAndReceive;

class Program
{
    class Options
    {
        public const string ENV_CONNECTION_STRING = "QUEUE_CONNECTION_STRING";

        public string? ConnectionString { get; set; }

        public string? QueueName { get; set; } = "q";

        public string? Message { get; set; } = "hello";

        public int Count { get; set; } = 1;

        public bool Debug { get; set; } = false;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                var connectionString = Environment.GetEnvironmentVariable(ENV_CONNECTION_STRING);
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ArgumentException($"ConnectionString cannot be empty. Set it in command line or in environment variable {ENV_CONNECTION_STRING}.",
                        nameof(ConnectionString));
                }
                ConnectionString = connectionString;
            }
            if (string.IsNullOrWhiteSpace(QueueName))
            {
                throw new ArgumentException("Cannot be blank.", nameof(QueueName));
            }
            if (string.IsNullOrEmpty(Message))
            {
                throw new ArgumentException("Cannot be empty.", nameof(Message));
            }
            if (Count < 1)
            {
                throw new ArgumentException($"Count is {Count}, less than 1.", nameof(Count));
            }
        }
    }

    static void ShowUsageAndExit(int exitCode = 0)
    {
        var usage = @"
Usage:
{0} --connect <queue connection string> [--queue <name>] [--message <content>] [--count <num>] [--debug] [--help | -h]

Note:
The connection string can also be set by environment variable {1}.
";
        Console.WriteLine(string.Format(usage, typeof(Program).Assembly.GetName().Name, Options.ENV_CONNECTION_STRING));
        Environment.Exit(exitCode);
    }

    static Options ParseCommandLine(string[] args)
    {
        var options = new Options();
        try
        {
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--connect":
                        options.ConnectionString = args[++i];
                        break;
                    case "--queue":
                        options.QueueName= args[++i];
                        break;
                    case "--message":
                        options.Message = args[++i];
                        break;
                    case "--count":
                        options.Count = int.Parse(args[++i]);
                        break;
                    case "--debug":
                        options.Debug = true;
                        break;
                    case "-h":
                    case "--help":
                        ShowUsageAndExit(0);
                        break;
                    default:
                        throw new ArgumentException("Unkown argument!", args[i]);
                }
            }
            options.Validate();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ShowUsageAndExit(1);
        }
        return options;
    }

    static int ReceivedCount = 0;
    static int CompletedCount = 0;

    static void SendAndReceiveMessages(ServiceBusClient client, Options options, ILogger logger)
    {
        logger.LogInformation("Sending '{message}' x {count}", options.Message, options.Count);
        var sender = client.CreateSender(options.QueueName);
        var message = new ServiceBusMessage(options.Message);
        var sendingTasks = new Task[options.Count];
        for (var i = 0; i < options.Count; i++)
        {
            sendingTasks[i] = sender.SendMessageAsync(message);
        }

        logger.LogInformation("Receiving messages");
        var receiver = client.CreateReceiver(options.QueueName);

        var receivingTasks = new Task[options.Count];
        for (var i = 0; i < options.Count; i++)
        {
            receivingTasks[i] = Task.Run(async () =>
            {
                var msg = await receiver.ReceiveMessageAsync(TimeSpan.MaxValue);
                Debug.Assert(msg != null);
                Interlocked.Increment(ref ReceivedCount);
                logger.LogDebug("{msg}:{body}", msg, msg.Body);
                await receiver.CompleteMessageAsync(msg);
                Interlocked.Increment(ref CompletedCount);
            });
        }

        var tasks = sendingTasks.Concat(receivingTasks).ToArray();
        try
        {
            Task.WaitAll(tasks);
        }
        finally
        {
            logger.LogInformation("Received: {ReceivedCount}", ReceivedCount);
            logger.LogInformation("Completed: {CompletedCount}", CompletedCount);
        }
    }

    static async Task Main(string[] args)
    {
        var options = ParseCommandLine(args);
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddSimpleConsole(options =>
            {
                options.UseUtcTimestamp = true;
                options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
            });
            if (options.Debug)
            {
                builder.SetMinimumLevel(LogLevel.Trace);
            }
        });
        var logger = loggerFactory.CreateLogger<Program>();
        await using var client = new ServiceBusClient(options.ConnectionString);
        SendAndReceiveMessages(client, options, logger);
    }
}
