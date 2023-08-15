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

/*module azureSSO 'azureSSO.bicep' = {
  name: '${deployment().name}-sso'
  params: {
    primaryStorageAccountName: '${take(replace(resourceBaseName, '-', ''), 11)}${uniqueString(rg.id)}'
    primaryStorageAccountLocation: location
  }
  scope: rg
}*/

module storageModule 'storage.bicep' = {
  name: '${deployment().name}-storage'
  params: {
    primaryStorageAccountName: '${take(replace(resourceBaseName, '-', ''), 11)}${uniqueString(rg.id)}'
    primaryStorageAccountLocation: location
  }
  scope: rg
}

module cognitiveServicesModule 'cognitiveServices.bicep' = {
  name: '${deployment().name}-cognitive'
  params: {
    servicesLocation: location
    searchServiceName: 'product-search-${uniqueString(rg.id)}'
    openAIserviceName: 'grocer-gpt-${uniqueString(rg.id)}'
    searchServiceSku: 'basic'
  }
  scope: rg
}

/*module configurationModule 'configuration.bicep' = {
  name: '${deployment().name}-configuration'
  params: {
    servicesLocation: location
  }
  scope: rg
}*/

module appServiceModule 'appService.bicep' = {
  name: '${deployment().name}-app'
  params: {
    webAppName: 'app-virtual-grocer-${uniqueString(rg.id)}'
    appServicePlanName: 'plan-virtual-grocer-${uniqueString(rg.id)}'
    webAppLocation: location
    appServiceSku: 'B1'
  }
  scope: rg
}
