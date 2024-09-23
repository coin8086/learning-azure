import { OsType, vmImageMap } from '../Shared/types-and-vars.bicep'

param vmSize string = 'Standard_DS2_v2'
param vmCount int = 2
param vmOsType OsType = 'windows'
param userName string
@secure()
param password string

@description('Azure Policy is the default and recommended way to setup VM Insights. But VM Insights can be setup in some other way. Use this parameter when Azure Policy is not preferred.')
param noPolicy bool = false

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
    isLinux: vmOsType == 'linux'
    userName: userName
    password: password
    dcrResId: noPolicy ? monitor.outputs.dcrResId : null
    userMiResId: noPolicy ? monitor.outputs.userMiResIdForAma : null
  }
  dependsOn: [
    monitor
  ]
}]
