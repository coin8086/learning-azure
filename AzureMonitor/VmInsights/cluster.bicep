param vmSize string = 'Standard_DS2_v2'
param vmImage object = {
  publisher: 'Canonical'
  offer: '0001-com-ubuntu-server-jammy'
  sku: '22_04-lts-gen2'
  version: 'latest'
}
param vmCount int = 2
param userName string
@secure()
param password string

@description('By default, Azure Policy is used for setting up VM Insights.')
param noPolicy bool = false

var location = resourceGroup().location

module monitor 'azure-monitor.bicep' = {
  name: 'monitor'
  params: {
    noPolicy: noPolicy
  }
}

resource vnet 'Microsoft.Network/virtualNetworks@2023-11-01' = {
  name: 'vnet'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
  }
}

resource defaulSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' = {
  parent: vnet
  name: 'default'
  properties: {
    addressPrefix: '10.0.0.0/22'
  }
}

module nodes 'node.bicep' = [for idx in range(1, vmCount): {
  name: 'node-${idx}'
  params: {
    name: 'node-${idx}'
    subnetResId: defaulSubnet.id
    vmSize: vmSize
    vmImage: vmImage
    isLinux: true
    userName: userName
    password: password
    dataCollectionRuleId: noPolicy ? monitor.outputs.dataCollectionRuleId : null
    userAssignedManagedIdentity: noPolicy ? monitor.outputs.userManagedIdentityIdForMA : null
  }
  dependsOn: [
    monitor
  ]
}]
