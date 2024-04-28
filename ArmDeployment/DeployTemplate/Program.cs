using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using System.Text.Json.Nodes;

namespace DeployTemplate;

class Program
{
    static void ShowUsage()
    {
        var usage = @"
Show tags of vms in a resource group of a subscription

{0} -s <subscription id> -g <resource group name> -t <ARM templete file path> -p <ARM template parameter file path>

";
        var exeName = typeof(Program).Assembly.GetName().Name;
        Console.WriteLine(string.Format(usage, exeName));
    }

    static (string sub, string rg, string template, string parameters) ParseArguments(string[] args)
    {
        string? sub = null;
        string? rg = null;
        string? template = null;
        string? parameters = null;
        try
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-s"))
                {
                    sub = args[++i];
                }
                else if (args[i].Equals("-g"))
                {
                    rg = args[++i];
                }
                else if (args[i].Equals("-t"))
                {
                    template = args[++i];
                }
                else if (args[i].Equals("-p"))
                {
                    parameters = args[++i];
                }
            }
            if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(rg) ||
                string.IsNullOrEmpty(template) || string.IsNullOrEmpty(parameters))
            {
                throw new ArgumentException();
            }
        }
        catch
        {
            ShowUsage();
            Environment.Exit(1);
        }

        return (sub, rg, template, parameters);
    }

    static async Task Main(string[] args)
    {
        var (sub, rg, templatePath, paramPath) = ParseArguments(args);

        // First we construct our client
        var client = new ArmClient(new DefaultAzureCredential());

        Console.WriteLine($"Subscription: {sub}");

        var subscriptions = client.GetSubscriptions();
        SubscriptionResource subscription = await subscriptions.GetAsync(sub);

        Console.WriteLine($"Resource group: {rg}");

        var resourceGroups = subscription.GetResourceGroups();
        ResourceGroupResource resourceGroup = await resourceGroups.GetAsync(rg);

        Console.WriteLine($"ARM template: {templatePath}");
        Console.WriteLine($"ARM template parameters: {paramPath}");

        var template = File.ReadAllText(templatePath);
        var parameters = File.ReadAllText(paramPath);

        //NOTE: The SDK doesn't accept a parameter file with schema like
        //  {
        //      "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
        //      "contentVersion": "1.0.0.0",
        //      "parameters": {
        //      }
        //  }
        //Only the content of "parameters" is accepted.

        var docRoot = JsonNode.Parse(parameters);
        var paramObj = docRoot!["parameters"];
        if (paramObj != null)
        {
            parameters = paramObj.ToJsonString();
        }

        var deploymentName = Guid.NewGuid().ToString();
        Console.WriteLine($"Deployment name: {deploymentName}");

        var deploymentData = new ArmDeploymentContent(new ArmDeploymentProperties(ArmDeploymentMode.Incremental)
        {
            Template = BinaryData.FromString(template),
            Parameters = BinaryData.FromString(parameters)
        });
        var deployments = resourceGroup.GetArmDeployments();

        Console.WriteLine("Deploying...");
        var result = await deployments.CreateOrUpdateAsync(Azure.WaitUntil.Completed, deploymentName, deploymentData);

        var deployment = result.Value;
        Console.WriteLine("Deployed resources:");
        foreach (var res in deployment.Data.Properties.OutputResources)
        {
            Console.WriteLine(res.Id);
        }
    }
}
