param name string = 'setup'
param location string = resourceGroup().location

var uniqStr = uniqueString(resourceGroup().id)
var prefix = '${name}${uniqStr}'
var dcrName = '${prefix}-dcr-logIngestionApi'

resource workSpace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${prefix}-workspace'
  location: location
}

module customTable 'custom-table.bicep' = {
  name: '${prefix}-customTable'
  params: {
    name: name
    workSpaceName: workSpace.name
  }
}

resource dce 'Microsoft.Insights/dataCollectionEndpoints@2023-03-11' = {
  name: '${prefix}-dce'
  location: location
  properties: {
  }
}

resource userMi 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${prefix}-userMi'
  location: location
}

module dcrModule 'data-collection-rule.bicep' = {
  name: '${prefix}-dcr'
  params: {
    dataCollectionRuleName: dcrName
    workspaceResId: workSpace.id
    dataCollectionEndpointId: dce.id
    userMiPrincipalIds: [
      userMi.properties.principalId
    ]
  }
  dependsOn: [
    customTable
  ]
}

output logsIngestionEndpoint string = dce.properties.logsIngestion.endpoint
output dcrRunId string = dcrModule.outputs.dcrRunId
output userMiResId string = userMi.id
output userMiClientId string = userMi.properties.clientId
