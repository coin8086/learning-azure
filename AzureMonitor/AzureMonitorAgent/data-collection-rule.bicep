
@description('Workspace Resource ID.')
param workspaceResourceId string

@description('Workspace Location.')
param workspaceLocation string

resource dcr 'Microsoft.Insights/dataCollectionRules@2021-04-01' = {
  name: 'dcrForSystemLog'
  location: workspaceLocation
  properties: {
    description: 'Data collection rule for Windows Event log or Linux Syslog'
    dataSources: {
      windowsEventLogs: [
        {
          name: 'SecurityEvents'
          streams: [
            'Microsoft-Event'
          ]
          scheduledTransferPeriod: 'PT1M'
          xPathQueries: [
            'Security!*'
          ]
        }
        {
          name: 'AppEvents'
          streams: [
            'Microsoft-Event'
          ]
          scheduledTransferPeriod: 'PT5M'
          xPathQueries: [
            'System!*[System[(Level = 1 or Level = 2 or Level = 3)]]'
            'Application!*[System[(Level = 1 or Level = 2 or Level = 3)]]'
          ]
        }
      ]
      syslog: [
        {
          name: 'cronSyslog'
          streams: [
            'Microsoft-Syslog'
          ]
          facilityNames: [
            'cron'
          ]
          logLevels: [
            'Debug'
            'Critical'
            'Emergency'
          ]
        }
        {
          name: 'syslogBase'
          streams: [
            'Microsoft-Syslog'
          ]
          facilityNames: [
            'syslog'
          ]
          logLevels: [
            'Alert'
            'Critical'
            'Emergency'
          ]
        }
      ]
    }
    destinations: {
      logAnalytics: [
        {
          workspaceResourceId: workspaceResourceId
          name: 'myWorkspace'
        }
      ]
    }
    dataFlows: [
      {
        streams: [
          'Microsoft-Syslog'
          'Microsoft-Event'
        ]
        destinations: [
          'myWorkspace'
        ]
      }
    ]
  }
}

output dcrResId string = dcr.id
