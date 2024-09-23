# Azure Monitor Agent with Data Collection Rule for system log

The sample code (in Bicep) shows how to install Azure Monitor Agent on VMs without using Azure Policy (which is buggy at the time of writing).

A Data Collection Rule for system log (Windows Event log or Linux Syslog) is also created and associated with each VM.

Refer to the following for more.

* https://learn.microsoft.com/en-us/azure/azure-monitor/agents/azure-monitor-agent-manage
* https://learn.microsoft.com/en-us/azure/azure-monitor/agents/data-collection-windows-events
* https://learn.microsoft.com/en-us/azure/azure-monitor/essentials/data-collection-rule-samples#azure-monitor-agent---events-and-performance-data