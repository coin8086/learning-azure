param vmSize string = 'Standard_DS2_v2'
param vmCount int = 2
param userName string
@secure()
param password string

var location = resourceGroup().location

module monitor 'azure-monitor-policy.bicep' = {
  name: 'monitor'
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
    userName: userName
    password: password
  }
  dependsOn: [
    monitor
  ]
}]
