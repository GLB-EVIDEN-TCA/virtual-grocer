﻿@description('Name for the Storage Account')
param primaryStorageAccountName string = 'genaipocstorage'

@description('Azure Location for the Storage Account')
param location string = resourceGroup().location

@description('URL for the Github Repository')
param repoUrl string = 'https://github.com/GLB-EVIDEN-TCA/virtual-grocer.git'

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
  name: 'deployscript-upload-product-index'
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
    scriptContent: 'git clone ${repoUrl}; cd virtual-grocer; git checkout feature/template; cd content/product-images; az storage blob upload-batch --account-name genaipocstorage -s . -d ecommerce-poc/product-images'
  }
}

output productContainerPath string = 'https://${primaryStorageAccountName}.blob.${environment().suffixes.storage}/products/images/'
