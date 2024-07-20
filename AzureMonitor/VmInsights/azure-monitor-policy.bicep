param name string = 'monitor'
param location string = resourceGroup().location

var uniqStr = uniqueString(resourceGroup().id)
var prefix = '${name}${uniqStr}'

resource workSpace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${prefix}-workspace'
  location: location
}

module dcr 'data-collection-rule.bicep' = {
  name: 'dataCollectionRules'
  params: {
    WorkspaceLocation: location
    WorkspaceResourceId: workSpace.id
  }
}

resource userMiForMa 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${prefix}-userMIForMA'
  location: location
}

resource userMiForVmPolicy 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${prefix}-userMIForVmPolicy'
  location: location
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(userMiForVmPolicy.id, 'Contributor')
  scope: resourceGroup()
  properties: {
    //Contributor role
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c')
    principalId: userMiForVmPolicy.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource vmPolicyForMa 'Microsoft.Authorization/policyAssignments@2023-04-01' = {
  name: '${prefix}-VMPolicyForMA'
  scope: resourceGroup()
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userMiForVmPolicy.id}': {}
    }
  }
  properties: {
    policyDefinitionId: '/providers/microsoft.authorization/policysetdefinitions/924bfe3a-762f-40e7-86dd-5c8b95eb09e6'
    displayName: 'Enable Azure Monitor for VMs with Azure Monitoring Agent'
    parameters: {
      enableProcessesAndDependencies: { value: true }
      bringYourOwnUserAssignedManagedIdentity: { value: true }
      userAssignedManagedIdentityName: { value: userMiForMa.name }
      userAssignedManagedIdentityResourceGroup: { value: resourceGroup().name }
      dcrResourceId: { value: dcr.outputs.dataCollectionRuleId }
    }
  }
  dependsOn: [
    roleAssignment
  ]
}

output laWorkSpaceId string = workSpace.id
output dataCollectionRuleId string = dcr.outputs.dataCollectionRuleId
output userManagedIdentityIdForMA string = userMiForMa.id
