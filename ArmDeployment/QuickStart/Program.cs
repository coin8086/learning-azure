//See https://learn.microsoft.com/en-us/dotnet/api/overview/azure/resourcemanager-readme?view=azure-dotnet

using Azure.ResourceManager.Resources;
using Azure.ResourceManager;
using Azure.Identity;
using Azure.ResourceManager.Compute;

namespace QuickStart;

class Program
{
    static void ShowUsage()
    {
        var usage = @"
Show tags of vms in a resource group of a subscription

{0} -s <subscription id> -g <resource group name>

";
        var exeName = typeof(Program).Assembly.GetName().Name;
        Console.WriteLine(string.Format(usage, exeName));
    }

    static (string sub, string rg) ParseArguments(string[] args)
    {
        string? sub = null;
        string? rg = null;
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
            }
            if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(rg))
            {
                throw new ArgumentException();
            }
        }
        catch
        {
            ShowUsage();
            Environment.Exit(1);
        }

        return (sub, rg);
    }

    public static async Task Main(string[] args)
    {
        var (sub, rg) = ParseArguments(args);

        // First we construct our client
        ArmClient client = new ArmClient(new DefaultAzureCredential());

        Console.WriteLine($"Get subscription {sub}.");

        //SubscriptionResource subscription = await client.GetDefaultSubscriptionAsync();
        SubscriptionCollection subscriptions = client.GetSubscriptions();
        SubscriptionResource subscription = await subscriptions.GetAsync(sub);

        Console.WriteLine($"Get resource group {rg}.");

        // Next we get a resource group object
        // ResourceGroupResource is a [Resource] object from above
        ResourceGroupCollection resourceGroups = subscription.GetResourceGroups();
        ResourceGroupResource resourceGroup = await resourceGroups.GetAsync(rg);

        // Next we get the collection for the virtual machines
        // vmCollection is a [Resource]Collection object from above
        VirtualMachineCollection virtualMachines = resourceGroup.GetVirtualMachines();

        // Next we loop over all vms in the collection
        // Each vm is a [Resource] object from above
        await foreach (VirtualMachineResource virtualMachine in virtualMachines)
        {
            //// We access the [Resource]Data properties from vm.Data
            //if (!virtualMachine.Data.Tags.ContainsKey("owner"))
            //{
            //    // We can also access all operations from vm since it is already scoped for us
            //    await virtualMachine.AddTagAsync("owner", "tagValue");
            //}

            Console.WriteLine($"VM {virtualMachine.Data.Name} tags:");

            foreach (var tag in virtualMachine.Data.Tags)
            {
                Console.WriteLine($"{tag.Key}={tag.Value}");
            }
        }
    }
}
