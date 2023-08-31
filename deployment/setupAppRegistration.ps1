param([string] $webAppName)

Connect-AzAccount -Identity

# Create app registration
$app = New-AzADApplication -DisplayName $webAppName -SigninAudience AzureADandPersonalMicrosoftAccount

# Set domain name and URLs
$DOMAIN_NAME = "$webAppName.azurewebsites.net"
$REPLY_URL_1 = "https://$DOMAIN_NAME/authentication/login-callback"
$REPLY_URL_1 = "https://$DOMAIN_NAME/authentication/login-callback"
$LOGOUT_URL = "https://$DOMAIN_NAME/signout-oidc"

# Update the application
Update-AzADApplication -ObjectId $CLIENT_ID -ReplyUrls $REPLY_URL_1 -LogoutUrl $LOGOUT_URL -Oauth2AllowImplicitFlow $true

$DeploymentScriptOutputs = @{}
$DeploymentScriptOutputs['clientId'] = $app.appId
