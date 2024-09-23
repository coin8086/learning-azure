param location string = resourceGroup().location

resource workSpace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'logWorkspace'
  location: location
}

module dcr 'data-collection-rule.bicep' = {
  name: 'dataCollectionRules'
  params: {
    workspaceLocation: location
    workspaceResourceId: workSpace.id
  }
}

resource userMiForMa 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'userMiForMa'
  location: location
}

output workSpaceResId string = workSpace.id
output dcrResId string = dcr.outputs.dcrResId
output userMiResIdForAma string = userMiForMa.id
