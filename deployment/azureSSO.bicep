@description('Name for the Web App')
@minLength(2)
@maxLength(60)
param webAppName string

@description('Azure Location for the Storage Account')
param location string = resourceGroup().location

param currentTime string = utcNow()

resource appRegistrationScriptIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${webAppName}-registration-identity'
  location: location
}

@description('This is the built-in Search Service Contributor role. See https://learn.microsoft.com/en-us/azure/active-directory/roles/permissions-reference#application-administrator')
resource applicationAdministratorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: tenant()
  name: '9b895d92-2cd3-44c7-9d02-a6ac2d5ea5c3'
}

resource applicationAdministratorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: tenant()
  name: guid(webAppName, appRegistrationScriptIdentity.id, applicationAdministratorRoleDefinition.id)
  properties: {
    roleDefinitionId: applicationAdministratorRoleDefinition.id
    principalId: appRegistrationScriptIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource setupAppRegistration 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: '${webAppName}-setup-app-registration'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${appRegistrationScriptIdentity.id}': {}
    }
  }
  kind: 'AzurePowerShell'
  properties: {
    azPowerShellVersion: '8.3'
    timeout: 'PT30M'
    arguments: '-webAppName \\"${webAppName}\\"'
    scriptContent: loadTextContent('setupAppRegistration.ps1')
    cleanupPreference: 'OnSuccess'
    retentionInterval: 'P1D'
    forceUpdateTag: currentTime // ensures script will run every time
  }
}

output clientId string = setupAppRegistration.properties.outputs.clientId
