targetScope = 'resourceGroup'

@description('Base name for the application and resources')
@minLength(2)
param resourceBaseName string = 'virtual-grocer'

@description('Azure location for all resources')
param location string = 'eastus'

var uniqueSuffix = uniqueString(resourceGroup().id)
var resourceName24 = take(resourceBaseName, 11)
// + uniqueSuffix

/*module azureSSO 'azureSSO.bicep' = {
  name: '${deployment().name}-sso'
  params: {
    primaryStorageAccountName: '${take(replace(resourceBaseName, '-', ''), 11)}${uniqueId}'
    primaryStorageAccountLocation: location
  }
}*/

module configurationModule 'configuration.bicep' = {
  name: '${deployment().name}-configuration'
  params: {
    keyVaultName: resourceName24
    location: location
  }
}

module storageModule 'storage.bicep' = {
  name: '${deployment().name}-storage'
  params: {
    primaryStorageAccountName: '${take(replace(resourceBaseName, '-', ''), 11)}${uniqueSuffix}'
    location: location
  }
}

module cognitiveSearchModule 'cognitiveSearch.bicep' = {
  name: '${deployment().name}-cognitive'
  params: {
    location: location
    searchServiceName: 'product-search-${uniqueSuffix}'
    searchServiceSku: 'basic'
    keyVaultName: resourceName24
  }
}

module openAImodule 'openAI.bicep' = {
  name: '${deployment().name}-openai'
  params: {
    location: location
    openAIserviceName: 'grocer-gpt-${uniqueSuffix}'
    keyVaultName: resourceName24
  }
}

module appServiceModule 'appService.bicep' = {
  name: '${deployment().name}-app'
  params: {
    webAppName: 'app-${resourceBaseName}-${uniqueSuffix}'
    appServicePlanName: 'plan-${resourceBaseName}-${uniqueSuffix}'
    webAppLocation: location
    appServiceSku: 'B1'
  }
}
