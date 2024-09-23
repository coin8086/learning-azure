type OsType = 'windows' | 'linux'

param vmSize string = 'Standard_DS2_v2'
param vmCount int = 2
param vmOsType OsType = 'windows'
param userName string
@secure()
param password string

@description('Azure Policy is the default and recommended way to setup VM Insights. But VM Insights can be setup in some other way. Use this parameter when Azure Policy is not preferred.')
param noPolicy bool = false

var vmImageMap = {
  windows: {
    publisher: 'MicrosoftWindowsServer'
    offer: 'WindowsServer'
    sku: '2019-Datacenter'
    version: 'latest'
  }

  //NOTE: the Dependency Extension doesn't support newer version of Ubuntu. So
  //Ubuntu 20.04 is used here. It's the latest supported version for the extension.
  linux: {
    publisher: 'Canonical'
    offer: '0001-com-ubuntu-server-focal'
    sku: '20_04-lts'
    version: 'latest'
  }
}
var vmImage = vmImageMap[vmOsType]

module monitor 'azure-monitor.bicep' = {
  name: 'monitor'
  params: {
    noPolicy: noPolicy
  }
}

module vnet '../Shared/vnet.bicep' = {
  name: 'vnet'
}

module nodes '../Shared/node.bicep' = [for idx in range(1, vmCount): {
  name: 'node-${idx}'
  params: {
    name: 'node-${idx}'
    subnetResId: vnet.outputs.subnetResId
    vmSize: vmSize
    vmImage: vmImage
    isLinux: true
    userName: userName
    password: password
    dcrResId: noPolicy ? monitor.outputs.dcrResId : null
    userMiResId: noPolicy ? monitor.outputs.userMiResIdForAma : null
  }
  dependsOn: [
    monitor
  ]
}]
