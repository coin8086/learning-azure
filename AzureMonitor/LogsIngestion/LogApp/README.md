# A NOTE for Azure Identity

The program uses [DefaultAzureCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet) as credential to access Log Analytics. So The entity represented by the credential must have the role "Monitoring Metrics Publisher" to write log to the LA.

There're some options to make such an entity.

One option is MS Entra App, as shown by the [Tuturial](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/tutorial-logs-ingestion-api?tabs=dcr). In this option, you need the following environment variables to use a MS Entra App for DefaultAzureCredential:

* AZURE_TENANT_ID
* AZURE_CLIENT_ID
* AZURE_CLIENT_SECRET

Another option is Managed Identity. The [Setup](../Setup/) uses this option and creates such a Managed Identity for your use. In this option, you need only one environment variable AZURE_CLIENT_ID, set to the Client ID of the Managed Identity. But you need to assign the Managed Identity to a VM before you create a `DefaultAzureCredential` instance from that VM.

For more about `DefaultAzureCredential`, see https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme?view=azure-dotnet