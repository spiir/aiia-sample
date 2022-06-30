cd Web

echo "Environment configuration script."

cp aiia.db.template aiia.db
echo "Reinitialized the Sqlite database 'aiia.db' from the template"

echo "We need to configure your clientId and secret to Aiia."
echo "Please enter them now and we'll set them up for you using 'dotnet user-secrets' (they will not appear in the configuration files)"

$clientId = Read-Host -Prompt "ClientId"
$clientSecret = Read-Host -Prompt "Secret"
$webHookSecret = Read-Host -Prompt "Webhook Secret"

dotnet user-secrets set "Aiia:ClientId" "$clientId"
dotnet user-secrets set "Aiia:ClientSecret" "$clientSecret"
dotnet user-secrets set "Aiia:WebHookSecret" "$webHookSecret"

cd ..