param([string] $webAppName)
$token = (Get-AzAccessToken -ResourceUrl https://graph.microsoft.com).Token
$headers = @{'Content-Type' = 'application/json'; 'Authorization' = 'Bearer ' + $token }

$domainName = "$webAppName.azurewebsites.net"
$replyUrl1 = "https://$domainName/authentication/login-callback"
$replyUrl2 = "https://localhost:44321/authentication/login-callback"
$logoutUrl = "https://$domainName/signout-oidc"

$template = @{
  name                   = $webAppName
  requiredResourceAccess = @(
    @{
      resourceAppId  = "00000003-0000-0000-c000-000000000000"
      resourceAccess = @(
        @{
          id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d"
          type = "Scope"
        }
      )
    }
  )
  signInAudience         = "AzureADandPersonalMicrosoftAccount"
  logoutUrl              = $logoutUrl
}
  
# Upsert App registration
$app = (Invoke-RestMethod -Method Get -Headers $headers -Uri "https://graph.microsoft.com/beta/applications?filter=displayName eq '$($webAppName)'").value
if ($app) {
  Invoke-RestMethod -Method Patch -Headers $headers -Uri "https://graph.microsoft.com/beta/applications/$($app.id)" -Body ($template | ConvertTo-Json -Depth 10)
}
else {
  $app = (Invoke-RestMethod -Method Post -Headers $headers -Uri "https://graph.microsoft.com/beta/applications" -Body ($template | ConvertTo-Json -Depth 10))
}
  
$DeploymentScriptOutputs = @{}
$DeploymentScriptOutputs['objectId'] = $app.id
$DeploymentScriptOutputs['clientId'] = $app.appId
