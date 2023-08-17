@description('Azure Location for the Storage Account')
param location string = resourceGroup().location

@description('Name for the Product Search Service')
param openAIserviceName string = 'grocer-gpt'

resource openAIaccount 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: openAIserviceName
  location: location
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

output openAIendpoint string = 'https://${openAIserviceName}.openai.azure.com'
