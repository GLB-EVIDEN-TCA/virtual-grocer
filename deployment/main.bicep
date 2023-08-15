targetScope = 'resourceGroup'

@description('Base name for the application and resources')
@minLength(2)
param resourceBaseName string = 'eviden-virtual-grocer'

@description('Azure location for all resources')
param location string = 'eastus'

var uniqueSuffix = uniqueString(resourceGroup().id)

/*module azureSSO 'azureSSO.bicep' = {
  name: '${deployment().name}-sso'
  params: {
    primaryStorageAccountName: '${take(replace(resourceBaseName, '-', ''), 11)}${uniqueId}'
    primaryStorageAccountLocation: location
  }
}*/

module storageModule 'storage.bicep' = {
  name: '${deployment().name}-storage'
  params: {
    primaryStorageAccountName: '${take(replace(resourceBaseName, '-', ''), 11)}${uniqueSuffix}'
    primaryStorageAccountLocation: location
  }
}

module cognitiveServicesModule 'cognitiveServices.bicep' = {
  name: '${deployment().name}-cognitive'
  params: {
    servicesLocation: location
    searchServiceName: 'product-search-${uniqueSuffix}'
    openAIserviceName: 'grocer-gpt-${uniqueSuffix}'
    searchServiceSku: 'basic'
  }
}

/*module configurationModule 'configuration.bicep' = {
  name: '${deployment().name}-configuration'
  params: {
    servicesLocation: location
  }
}*/

module appServiceModule 'appService.bicep' = {
  name: '${deployment().name}-app'
  params: {
    webAppName: 'app-virtual-grocer-${uniqueSuffix}'
    appServicePlanName: 'plan-virtual-grocer-${uniqueSuffix}'
    webAppLocation: location
    appServiceSku: 'B1'
  }
}
