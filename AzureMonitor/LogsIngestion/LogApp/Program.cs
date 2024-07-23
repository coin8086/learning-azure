//See https://learn.microsoft.com/en-us/azure/azure-monitor/logs/tutorial-logs-ingestion-code?tabs=net

using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Ingestion;

namespace LogApp;

class Program
{
    static async Task Main(string[] args)
    {
        //A URL like "https://xxx.ingest.monitor.azure.com"
        var dceURL = Environment.GetEnvironmentVariable("LOGAPP_DCE") ?? throw new ArgumentException("Environment variable LOGAPP_DCE is not found!");
        //An ID like "dcr-5b825a783cdf4dc58784040be0b18284"
        var dcrID = Environment.GetEnvironmentVariable("LOGAPP_DCR") ?? throw new ArgumentException("Environment variable LOGAPP_DCR is not found!");
        var streamName = "Custom-MyTableRawData";

        // Create credential and client
        var credential = new DefaultAzureCredential();
        LogsIngestionClient client = new(new Uri(dceURL), credential);

        DateTimeOffset currentTime = DateTimeOffset.UtcNow;

        // Use BinaryData to serialize instances of an anonymous type into JSON
        BinaryData data = BinaryData.FromObjectAsJson(
            new[] {
                new
                {
                    Time = currentTime,
                    Computer = "Computer1",
                    AdditionalContext = new
                    {
                        InstanceName = "user1",
                        TimeZone = "Pacific Time",
                        Level = 4,
                        CounterName = "AppMetric1",
                        CounterValue = 15.3
                    }
                },
                new
                {
                    Time = currentTime,
                    Computer = "Computer2",
                    AdditionalContext = new
                    {
                        InstanceName = "user2",
                        TimeZone = "Central Time",
                        Level = 3,
                        CounterName = "AppMetric1",
                        CounterValue = 23.5
                    }
                },
            });

        // Upload logs
        try
        {
            var response = await client.UploadAsync(dcrID, streamName, RequestContent.Create(data)).ConfigureAwait(false);
            if (response.IsError)
            {
                throw new Exception(response.ToString());
            }

            Console.WriteLine("Log upload completed using content upload");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Upload failed with Exception: " + ex.Message);
        }

        // Logs can also be uploaded in a List
        var entries = new List<object>();
        for (int i = 0; i < 10; i++)
        {
            entries.Add(
                new
                {
                    Time = currentTime,
                    Computer = "Computer" + i.ToString(),
                    AdditionalContext = new
                    {
                        InstanceName = "user" + i.ToString(),
                        TimeZone = "Central Time",
                        Level = 3,
                        CounterName = "AppMetric1" + i.ToString(),
                        CounterValue = i
                    }
                }
            );
        }

        // Make the request
        try
        {
            var response = await client.UploadAsync(dcrID, streamName, entries).ConfigureAwait(false);
            if (response.IsError)
            {
                throw new Exception(response.ToString());
            }

            Console.WriteLine("Log upload completed using list of entries");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Upload failed with Exception: " + ex.Message);
        }
    }
}
