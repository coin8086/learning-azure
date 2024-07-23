param dataCollectionRuleName string
param location string = resourceGroup().location
param workspaceResId string
param dataCollectionEndpointId string?
param userMiPrincipalIds string[] = []

resource dcr 'Microsoft.Insights/dataCollectionRules@2023-03-11' = {
  name: dataCollectionRuleName
  location: location
  kind: 'Direct'
  properties: {
    dataCollectionEndpointId: dataCollectionEndpointId
    streamDeclarations: {
      'Custom-MyTableRawData': {
        columns: [
          {
            name: 'Time'
            type: 'datetime'
          }
          {
            name: 'Computer'
            type: 'string'
          }
          {
            name: 'AdditionalContext'
            type: 'string'
          }
          {
            name: 'CounterName'
            type: 'string'
          }
          {
            name: 'CounterValue'
            type: 'real'
          }
        ]
      }
    }
    destinations: {
      logAnalytics: [
        {
          workspaceResourceId: workspaceResId
          name: 'myworkspace'
        }
      ]
    }
    dataFlows: [
      {
        streams: [
          'Custom-MyTableRawData'
        ]
        destinations: [
          'myworkspace'
        ]
        transformKql: 'source | extend jsonContext = parse_json(AdditionalContext) | project TimeGenerated = Time, Computer, AdditionalContext = jsonContext, CounterName=tostring(jsonContext.CounterName), CounterValue=toreal(jsonContext.CounterValue)'
        outputStream: 'Custom-MyTable_CL'
      }
    ]
  }
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for (id, index) in userMiPrincipalIds: {
  name: guid(id, 'Monitoring Metrics Publisher')
  scope: dcr
  properties: {
    //Monitoring Metrics Publisher
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '3913510d-42f4-4e42-8a64-420c390055eb')
    principalId: id
    principalType: 'ServicePrincipal'
  }
}]

output dcrResId string = dcr.id
output dcrRunId string = dcr.properties.immutableId
