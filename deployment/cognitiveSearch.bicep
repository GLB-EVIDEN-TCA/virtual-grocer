@description('Azure Location for the Storage Account')
param location string = resourceGroup().location

@description('Name for the Product Search Service')
@minLength(2)
@maxLength(60)
param searchServiceName string

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

@description('Specifies the name of the key vault.')
param keyVaultName string

resource searchService 'Microsoft.Search/searchServices@2022-09-01' = {
  name: searchServiceName
  location: location
  sku: {
    name: searchServiceSku
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
    publicNetworkAccess: 'enabled'
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

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyVaultName
}

resource searchServiceKey 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'cognitive-search-key'
  properties: {
    contentType: 'Cognitive Search Key'
    value: searchService.listQueryKeys().value[0].key
  }
} 

// Deployment script for creating an index and uploading data
resource setupIndexAndUploadData 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: '${searchServiceName}-setup-index'
  location: location
  kind: 'AzurePowerShell'
  properties: {
    azPowerShellVersion: '8.3'
    timeout: 'PT30M'
    environmentVariables: [
      {
        name: 'CONTENT'
        value: loadTextContent('../content/products/products-generic.json')
      }
      {
        name: 'SEARCH_SERVICE_ADMIN_KEY'
        secureValue: searchService.listAdminKeys().primaryKey
      }
    ]
    arguments: '-searchServiceName \\"${searchServiceName}\\" -indexName \\"${searchServiceIndexName}\\"'
    scriptContent: '''
      param(
        [string]$searchServiceName,
        [string]$indexName
      )

      $searchServiceKey = $env:SEARCH_SERVICE_ADMIN_KEY
      
      # Define the index schema
      $indexDefinition = @{
        "name" = $indexName;
        "fields" = @(
            @{
                "name" = "id";
                "type" = "Edm.String";
                "key" = $true;
                "filterable" = $false;
                "sortable" = $false;
                "retrievable" = $true;
                "facetable" = $false;
            },
            @{
                "name" = "name";
                "type" = "Edm.String";
                "searchable" = $true;
                "filterable" = $true;
                "sortable" = $true;
                "retrievable" = $true;
                "facetable" = $false;
            },
            @{
                "name" = "aliases";
                "type" = "Collection(Edm.String)";
                "searchable" = $true;
                "filterable" = $false;
                "sortable" = $false;
                "retrievable" = $false;
                "facetable" = $false;
            },
            @{
                "name" = "size";
                "type" = "Edm.String";
                "searchable" = $true;
                "filterable" = $false;
                "sortable" = $true;
                "retrievable" = $true;
                "facetable" = $false;
            },
            @{
                "name" = "image_name";
                "type" = "Edm.String";
                "searchable" = $false;
                "filterable" = $false;
                "sortable" = $false;
                "retrievable" = $true;
                "facetable" = $false;
            },
            @{
                "name" = "cost";
                "type" = "Edm.Double";
                "searchable" = $false;
                "filterable" = $false;
                "sortable" = $true;
                "retrievable" = $true;
                "facetable" = $false;
            }
        )
      }

      #Check if the $indexName index already exists, if it does not, create it
      try {
        # Check if the $indexName index already exists
        $indexExists = Invoke-RestMethod -Method Get -Uri "https://$searchServiceName.search.windows.net/indexes/$indexName\?api-version=2020-06-30" -Headers @{"api-key" = $searchServiceKey}
    
        # If no error, index exists, but this logic won't be used, it's here for clarity
        Write-Host "Index $indexName already exists"
      }
      catch {
          # If error, check if it's a 404
          if ($_.Exception.Response.StatusCode -eq 'NotFound') {
              Write-Host "Index $indexName does not exist, creating it now"
              
              Invoke-RestMethod -Method Post -Uri "https://$searchServiceName.search.windows.net/indexes?api-version=2020-06-30" -Body (ConvertTo-Json -InputObject $indexDefinition) -Headers @{
                "api-key" = $searchServiceKey
                "Content-Type" = "application/json"
              }
          }
          else {
              # If error is something other than 404
              throw $_
          }
      }

      $jsonContent = $env:CONTENT

      Invoke-RestMethod -Method Post -Uri "https://$searchServiceName.search.windows.net/indexes/$indexName/docs/index?api-version=2020-06-30" -Body $jsonContent -Headers @{
          "api-key" = $searchServiceKey;
          "Content-Type" = "application/json";
      }
    '''
    cleanupPreference: 'OnSuccess'
    retentionInterval: 'P1D'
  }
}
