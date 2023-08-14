targetScope = 'subscription'

@description('Base name for the application and all resources')
@minLength(2)
param resourceBaseName string = 'eviden-virtual-grocer'

@description('Azure Location for all resources')
param location string = 'eastus'

resource rg 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceBaseName
  location: location
}

module storageModule 'storage.bicep' = {
  name: '${deployment().name}-storage'
  params: {
    primaryStorageAccountName: '${take(replace(resourceBaseName, '-', ''), 11)}${uniqueString(rg.id)}'
    primaryStorageAccountLocation: location
  }
  scope: rg
}

/*module cognitiveServicesModule 'cognitiveServices.bicep' = {
  name: '${deployment().name}-cognitiveServices'
  params: {}
  scope: rg
}*/
