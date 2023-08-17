@description('Name for the Storage Account')
param primaryStorageAccountName string

@description('Azure Location for the Storage Account')
param location string = resourceGroup().location

@description('UTC timestamp used to create distinct deployment scripts for each deployment')
param utcValue string = utcNow()

resource primaryStorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: primaryStorageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    dnsEndpointType: 'Standard'
    defaultToOAuthAuthentication: false
    publicNetworkAccess: 'Enabled'
    allowCrossTenantReplication: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: true
    allowSharedKeyAccess: true
    networkAcls: {
      bypass: 'AzureServices'
      virtualNetworkRules: []
      ipRules: []
      defaultAction: 'Allow'
    }
    supportsHttpsTrafficOnly: true
    encryption: {
      requireInfrastructureEncryption: false
      services: {
        file: {
          keyType: 'Account'
          enabled: true
        }
        blob: {
          keyType: 'Account'
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
    accessTier: 'Hot'
  }
}

resource primaryStorageAccountBlob 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
  parent: primaryStorageAccount
  name: 'default'
  properties: {
    changeFeed: {
      enabled: false
    }
    restorePolicy: {
      enabled: false
    }
    containerDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    cors: {
      corsRules: []
    }
    deleteRetentionPolicy: {
      allowPermanentDelete: false
      enabled: true
      days: 7
    }
    isVersioningEnabled: false
  }
}

resource productsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  parent: primaryStorageAccountBlob
  name: 'products'
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'Blob'
  }
}

resource deploymentScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'deployscript-upload-blob-${utcValue}'
  location: location
  kind: 'AzureCLI'
  properties: {
    azCliVersion: '2.26.1'
    timeout: 'PT5M'
    retentionInterval: 'PT1H'
    environmentVariables: [
      {
        name: 'AZURE_STORAGE_ACCOUNT'
        value: primaryStorageAccount.name
      }
      {
        name: 'AZURE_STORAGE_KEY'
        secureValue: primaryStorageAccount.listKeys().keys[0].value
      }
      {
        name: 'CONTENT'
        value: loadTextContent('../content/products/products-generic.json')
      }
    ]
    scriptContent: 'echo "$CONTENT" > products-generic.json && az storage blob upload -f products-generic.json -c products -n "index\\products-generic.json"'
  }
}

output productContainerPath string = 'https://${primaryStorageAccountName}.blob.${environment().suffixes.storage}/products/images/'
