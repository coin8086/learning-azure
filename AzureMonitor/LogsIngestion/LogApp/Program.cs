//See https://learn.microsoft.com/en-us/azure/azure-monitor/logs/tutorial-logs-ingestion-code?tabs=net

using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Ingestion;

namespace LogApp;

class Program
{
    static async Task Main(string[] args)
    {
        // Initialize variables
        var endpoint = new Uri("https://leiz-la-api-dev-sfty.southeastasia-1.ingest.monitor.azure.com");    //Your value
        var ruleId = "dcr-7f42bbeef83b4d759d634e2b0daa11b4";    //Your value
        var streamName = "Custom-MyTableRawData";

        // Create credential and client
        var credential = new DefaultAzureCredential();
        LogsIngestionClient client = new(endpoint, credential);

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
            var response = await client.UploadAsync(ruleId, streamName, RequestContent.Create(data)).ConfigureAwait(false);
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
            var response = await client.UploadAsync(ruleId, streamName, entries).ConfigureAwait(false);
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
