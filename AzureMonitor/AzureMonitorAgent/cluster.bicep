import { OsType, vmImageMap } from '../Shared/types-and-vars.bicep'

param vmSize string = 'Standard_DS2_v2'
param vmCount int = 2
param vmOsType OsType = 'windows'
param userName string
@secure()
param password string

var vmImage = vmImageMap[vmOsType]

module monitor 'azure-monitor.bicep' = {
  name: 'monitor'
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
    dcrResId: monitor.outputs.dcrResId
    userMiResId: monitor.outputs.userMiResIdForAma
  }
  dependsOn: [
    monitor
  ]
}]
