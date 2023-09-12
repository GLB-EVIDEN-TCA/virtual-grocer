targetScope = 'resourceGroup'

@description('Azure location for all resources')
param location string = resourceGroup().location

@description('Base name for the application and resources')
@minLength(3)
param resourceBaseName string = 'virtual-grocer-${uniqueString(resourceGroup().id)}'

@description('Specifies the name of the key vault.')
@minLength(3)
@maxLength(24)
param keyVaultName string = 'grocer-vlt-${uniqueString(resourceGroup().id)}'

@description('Name for the Storage Account')
@minLength(3)
@maxLength(24)
param storageAccountName string = 'grocerstor${uniqueString(resourceGroup().id)}'

@description('Name for the Product Search Service')
@minLength(2)
@maxLength(60)
param searchServiceName string = 'product-search-${uniqueString(resourceGroup().id)}'

@description('Name for the Product Search Service Index to store the Inventory Data')
param searchServiceIndexName string = 'product-inventory'

@description('The pricing tier of the search service you want to create (for example, basic or standard).')
@allowed([
  'free'
  'basic'
  'standard'
  'standard2'
  'standard3'
  'storage_optimized_l1'
  'storage_optimized_l2'
])
param searchServiceSku string = 'basic'

@description('Name for the OpenAI Chat Service')
@minLength(2)
@maxLength(64)
param openAIserviceName string = 'grocer-gpt-${uniqueString(resourceGroup().id)}'

@description('Name for the Web App')
@minLength(2)
@maxLength(60)
param webAppName string = 'app-${resourceBaseName}'

@description('Name for the App Service Plan')
@minLength(2)
@maxLength(60)
param appServicePlanName string = 'plan-${resourceBaseName}'

@description('The SKU of App Service Plan.')
param appServiceSku string = 'B1'

/*module azureSSO 'azureSSO.bicep' = {
  name: '${deployment().name}-sso'
  params: {
    location: location
  }
}*/

module configurationModule 'configuration.bicep' = {
  name: '${deployment().name}-configuration'
  params: {
    keyVaultName: keyVaultName
    location: location
  }
}

module ssoModule 'azureSSO.bicep' = {
  name: '${deployment().name}-sso'
  params: {
    webAppName: webAppName
    location: location
  }
}

module storageModule 'storage.bicep' = {
  name: '${deployment().name}-storage'
  params: {
    storageAccountName: storageAccountName
    location: location
  }
}

module cognitiveSearchModule 'cognitiveSearch.bicep' = {
  name: '${deployment().name}-cognitive'
  params: {
    location: location
    searchServiceName: searchServiceName
    searchServiceIndexName: searchServiceIndexName
    searchServiceSku: searchServiceSku
    keyVaultName: keyVaultName
  }
  dependsOn: [
    configurationModule
  ]
}

module openAImodule 'openAI.bicep' = {
  name: '${deployment().name}-openai'
  params: {
    location: location
    openAIserviceName: openAIserviceName
    keyVaultName: keyVaultName
  }
  dependsOn: [
    configurationModule
  ]
}

module appServiceModule 'appService.bicep' = {
  name: '${deployment().name}-app'
  params: {
    webAppName: webAppName
    appServicePlanName: appServicePlanName
    webAppLocation: location
    appServiceSku: appServiceSku
    keyVaultName: keyVaultName
    storageAccountName: storageAccountName
    searchServiceName: searchServiceName
    searchServiceIndexName: searchServiceIndexName
    openAIserviceName: openAIserviceName
  }
  dependsOn: [
    configurationModule
    openAImodule
  ]
}

output appServiceName string = webAppName
