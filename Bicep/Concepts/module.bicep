param name string = 'leizstoragedev3'

module storageModule 'storage.bicep' = {
  name: 'storage-deployment'
  params: {
    location: 'southeastasia'
    name: name
  }
}

output x object = storageModule.outputs.endPoints
