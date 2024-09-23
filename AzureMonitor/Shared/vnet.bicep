param location string = resourceGroup().location
param vnetName string = 'vnet'
param subnetName string = 'defaultSubnet'

resource vnet 'Microsoft.Network/virtualNetworks@2023-11-01' = {
  name: vnetName
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
  name: subnetName
  properties: {
    addressPrefix: '10.0.0.0/22'
  }
}

output vnetResId string = vnet.id
output subnetResId string = defaulSubnet.id
