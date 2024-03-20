param name string

@allowed([
  'southeastasia'
  'japanwest'
  'japaneast'
  'eastus'
  'eastus2'
])
param location string

param sku string = 'Standard_LRS'

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: name
  location: location
  sku: {
    name: sku
  }
  kind: 'StorageV2'
}

output endPoints object = storage.properties.primaryEndpoints
