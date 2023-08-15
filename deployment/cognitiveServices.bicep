@description('Azure Location for the Storage Account')
param servicesLocation string = resourceGroup().location

@description('Name for the Product Search Service')
param searchServiceName string = 'product-search'

@allowed([
  'free'
  'basic'
  'standard'
  'standard2'
  'standard3'
  'storage_optimized_l1'
  'storage_optimized_l2'
])
@description('The pricing tier of the search service you want to create (for example, basic or standard).')
param searchServiceSku string = 'basic'

@description('Name for the Product Search Service')
param openAIserviceName string = 'grocer-gpt'

resource searchService 'Microsoft.Search/searchServices@2022-09-01' = {
  name: searchServiceName
  location: servicesLocation
  sku: {
    name: searchServiceSku
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
    publicNetworkAccess: 'enabled'
    networkRuleSet: {
      ipRules: []
    }
    encryptionWithCmk: {
      enforcement: 'Unspecified'
    }
    disableLocalAuth: false
    authOptions: {
      aadOrApiKey: {
        aadAuthFailureMode: 'http401WithBearerChallenge'
      }
    }
  }
}

resource openAIaccount 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: openAIserviceName
  location: servicesLocation
  sku: {
    name: 'S0'
  }
  kind: 'OpenAI'
  properties: {
    customSubDomainName: openAIserviceName
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
    publicNetworkAccess: 'Enabled'
  }
}

resource openAIdeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: openAIaccount
  name: 'virtual-grocer-chat'
  sku: {
    name: 'Standard'
    capacity: 30
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'text-davinci-003'
      version: '1'
    }
  }
}

output searchEndpoint string = 'https://${searchServiceName}.search.windows.net'
output openAIendpoint string = 'https://${openAIserviceName}.openai.azure.com'
