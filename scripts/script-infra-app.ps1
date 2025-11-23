Param(
    [string]$Prefix = "loginapi",       # prefixo pra nomear recursos
    [string]$Location = "brazilsouth"   # região da Azure
)

# --- Validações básicas ---
if (-not $env:AZ_SQL_ADMIN_LOGIN -or -not $env:AZ_SQL_ADMIN_PASSWORD) {
    Write-Error "As variáveis de ambiente AZ_SQL_ADMIN_LOGIN e AZ_SQL_ADMIN_PASSWORD precisam estar definidas."
    exit 1
}

# Pega subscription atual (opcional, só pra log)
$subscriptionId = $env:AZ_SUBSCRIPTION_ID
if (-not $subscriptionId) {
    $subscriptionId = az account show --query id -o tsv
}
Write-Host "Usando subscription: $subscriptionId"

# --- Nomes dos recursos (derivados do prefixo) ---
$rgName      = "$Prefix-rg"
$sqlServer   = "$Prefix-sqlsrv"
$sqlDb       = "$Prefix-db"
$appPlan     = "$Prefix-asp"
$webAppName  = "$Prefix-webapi"

Write-Host "Criando recursos com prefixo: $Prefix"
Write-Host "Resource Group: $rgName"
Write-Host "SQL Server: $sqlServer"
Write-Host "SQL DB: $sqlDb"
Write-Host "App Service Plan: $appPlan"
Write-Host "Web App: $webAppName"
Write-Host ""

# --- 1) Resource Group ---
Write-Host "==> Criando Resource Group..."
az group create `
  --name $rgName `
  --location $Location `
  --output table

# --- 2) SQL Server ---
Write-Host "==> Criando SQL Server..."
az sql server create `
  --name $sqlServer `
  --resource-group $rgName `
  --location $Location `
  --admin-user $env:AZ_SQL_ADMIN_LOGIN `
  --admin-password $env:AZ_SQL_ADMIN_PASSWORD `
  --output table

# --- 3) Regra de firewall para permitir serviços Azure ---
Write-Host "==> Criando regra de firewall para acesso a partir dos serviços Azure..."
az sql server firewall-rule create `
  --resource-group $rgName `
  --server $sqlServer `
  --name "AllowAzureServices" `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 0.0.0.0 `
  --output table

# --- 4) Banco de dados SQL ---
Write-Host "==> Criando banco de dados SQL..."
az sql db create `
  --resource-group $rgName `
  --server $sqlServer `
  --name $sqlDb `
  --service-objective S0 `
  --output table

# --- 5) App Service Plan ---
Write-Host "==> Criando App Service Plan..."
az appservice plan create `
  --name $appPlan `
  --resource-group $rgName `
  --location $Location `
  --sku B1 `
  --output table

# --- 6) Web App (.NET) ---
Write-Host "==> Criando Web App..."
az webapp create `
  --name $webAppName `
  --resource-group $rgName `
  --plan $appPlan `
  --runtime "DOTNET:8" `
  --output table

# --- 7) String de conexão do Web App ---
Write-Host "==> Configurando string de conexão no Web App..."
# Monta a connection string padrão (ajuste o nome 'DefaultConnection' se sua API usar outro)
$connectionString = "Server=tcp:$sqlServer.database.windows.net,1433;Initial Catalog=$sqlDb;User ID=$($env:AZ_SQL_ADMIN_LOGIN);Password=$($env:AZ_SQL_ADMIN_PASSWORD);Encrypt=True;Connection Timeout=30;"

az webapp config connection-string set `
  --resource-group $rgName `
  --name $webAppName `
  --settings DefaultConnection=$connectionString `
  --connection-string-type SQLAzure `
  --output table

Write-Host ""
Write-Host "==============================="
Write-Host "Provisionamento concluído!"
Write-Host "Resource Group: $rgName"
Write-Host "Web App: $webAppName"
Write-Host "SQL Server: $sqlServer"
Write-Host "SQL DB: $sqlDb"
Write-Host "==============================="