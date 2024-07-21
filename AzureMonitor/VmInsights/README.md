# Enable VM Insights

The sample code (in Bicep) shows how to enable VM Insights for a cluster of VMs. It demonstrates two ways for this: using Azure Policy or setting up Azure Monitor Agent (AMA) directly on VMs.

The former way is recommended for production environment, since it's easy to deploy and scale as the number of nodes in the cluster (resource group) changes. But the policy needs some time to complete setting up AMA after the ARM deployment is completed. The time depends and it takes no more than 1 hour in my experience.

The latter way is recommended for dev and test. It's not scalable but it's quick: the AMA is installed directly on VMs in the ARM deployment. So we can ensure it's available right after the deployment.
