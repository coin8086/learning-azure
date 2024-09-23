@export()
type OsType = 'windows' | 'linux'

@export()
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
