param name string
param subnetResId string
param vmSize string
param vmImage object
param isLinux bool
param vmDiskSizeInGB int = 64
param userName string
@secure()
param password string
param dataCollectionRuleId string?
param userAssignedManagedIdentity string?

var location = resourceGroup().location
var setupAMA = !empty(dataCollectionRuleId) && !empty(userAssignedManagedIdentity)

resource ip 'Microsoft.Network/publicIPAddresses@2023-11-01' = {
  name: '${name}-ip'
  location: location
  properties: {
    publicIPAllocationMethod: 'Dynamic'
  }
}

resource nic 'Microsoft.Network/networkInterfaces@2023-11-01' = {
  name: '${name}-nic'
  location: location
  properties: {
    ipConfigurations: [
      {
        name: 'IPConfig'
        properties: {
          subnet: {
            id: subnetResId
          }
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: {
            id: ip.id
          }
        }
      }
    ]
  }
}

resource vm 'Microsoft.Compute/virtualMachines@2024-03-01' = {
  name: name
  location: location
  properties: {
    hardwareProfile: {
      vmSize: vmSize
    }
    osProfile: {
      computerName: name
      adminUsername: userName
      adminPassword: password
    }
    storageProfile: {
      imageReference: vmImage
      osDisk: {
        name: '${name}-osdisk'
        createOption: 'FromImage'
        diskSizeGB: vmDiskSizeInGB
        caching: 'ReadOnly'
        managedDisk: {
          storageAccountType: 'StandardSSD_LRS'
        }
      }
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: nic.id
        }
      ]
    }
  }
}

module linuxAMA 'ama-linux.bicep' = if (setupAMA && isLinux) {
  name: 'linuxAMA'
  params: {
    dataCollectionRuleId: dataCollectionRuleId!
    userAssignedManagedIdentity: userAssignedManagedIdentity!
    vmName: vm.name
  }
}

module windowsAMA 'ama-windows.bicep' = if (setupAMA && !isLinux) {
  name: 'windowsAMA'
  params: {
    dataCollectionRuleId: dataCollectionRuleId!
    userAssignedManagedIdentity: userAssignedManagedIdentity!
    vmName: vm.name
  }
}
