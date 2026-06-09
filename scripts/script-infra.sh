#!/bin/bash
# ==============================================================================
# GLOBAL SOLUTION - ADVANCED BUSINESS DEVELOPMENT WITH .NET
# SCRIPT DE PROVISIONAMENTO DE INFRAESTRUTURA NA AZURE (AZURE CLI)
# PREFIXO OBRIGATÓRIO: script-infra
# PROJETO: AgroGuard
# ==============================================================================

# Definição de Variáveis (Modifique se necessário)
RESOURCE_GROUP="rg-agroguard-gs"
LOCATION="eastus"
PLAN_NAME="plan-agroguard"
WEBAPP_NAME="webapp-agroguard-$RANDOM" # Nome dinâmico para garantir unicidade
DB_CONTAINER_NAME="aci-agroguard-db"
DB_DNS_LABEL="agroguard-db-$RANDOM"
DB_PASSWORD="AgroGuard123Password" # Senha de administrador do Oracle (SYS/SYSTEM)
APP_DB_USER="AGROGUARD"
APP_DB_PASSWORD="AgroGuard123"     # Senha utilizada pela aplicação .NET

echo "=== 1. Iniciando Login na Azure ==="
# O login deve ser feito previamente pelo terminal usando 'az login'.
# Este script assume que você já está autenticado na assinatura desejada.

echo "=== 2. Criando Resource Group ==="
az group create --name $RESOURCE_GROUP --location $LOCATION

echo "=== 3. Criando Container do Banco de Dados Oracle (ACI) ==="
# Provisionando um container com gvenzl/oracle-free (leve, próprio para desenvolvimento)
# As variáveis APP_USER e APP_USER_PASSWORD criam o usuário AGROGUARD automaticamente.
az container create \
  --resource-group $RESOURCE_GROUP \
  --name $DB_CONTAINER_NAME \
  --image gvenzl/oracle-free:23-slim \
  --cpu 1 \
  --memory 2 \
  --ports 1521 \
  --environment-variables \
    ORACLE_PASSWORD="$DB_PASSWORD" \
    APP_USER="$APP_DB_USER" \
    APP_USER_PASSWORD="$APP_DB_PASSWORD" \
  --ip-address public \
  --dns-name-label $DB_DNS_LABEL \
  --restart-policy OnFailure

echo "=== 4. Obtendo Informações de Conexão do Banco ==="
# Aguarda o provisionamento e obtém o IP Público ou FQDN
DB_HOST=$(az container show --resource-group $RESOURCE_GROUP --name $DB_CONTAINER_NAME --query ipAddress.fqdn --output tsv)
if [ -z "$DB_HOST" ]; then
    DB_HOST=$(az container show --resource-group $RESOURCE_GROUP --name $DB_CONTAINER_NAME --query ipAddress.ip --output tsv)
fi
echo "Banco de dados disponível no endereço: $DB_HOST"

# String de Conexão formatada para Oracle Entity Framework
CONNECTION_STRING="User Id=$APP_DB_USER;Password=$APP_DB_PASSWORD;Data Source=$DB_HOST:1521/FREEPDB1;"
echo "Connection String gerada (ocultando credenciais): User Id=$APP_DB_USER;Data Source=$DB_HOST:1521/FREEPDB1;"

echo "=== 5. Criando App Service Plan (Linux) ==="
# Criando no plano Standard B1 (ou F1 Free, se suportado na região)
az appservice plan create \
  --name $PLAN_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --is-linux \
  --sku B1

echo "=== 6. Criando o Web App (.NET 8) ==="
az webapp create \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan $PLAN_NAME \
  --runtime "DOTNET:8"

echo "=== 7. Configurando Connection Strings do Web App ==="
# Configura a Connection String de forma segura como variável de ambiente na nuvem
az webapp config connection-string set \
  --resource-group $RESOURCE_GROUP \
  --name $WEBAPP_NAME \
  --connection-string-type Custom \
  --settings "Oracle=$CONNECTION_STRING"

echo "=== 8. Configurando Variáveis de Ambiente e Jwt Secret ==="
# Protegendo dados sensíveis e configurando variáveis de ambiente
az webapp config appsettings set \
  --resource-group $RESOURCE_GROUP \
  --name $WEBAPP_NAME \
  --settings \
    Jwt__Secret="AgroGuardSatellite_JwtSecret_For_Academic_Demo_2026_SecureKey" \
    Jwt__Issuer="AgroGuardSatellite" \
    Jwt__Audience="AgroGuardSatellite.Client" \
    Jwt__ExpirationMinutes=120 \
    ASPNETCORE_ENVIRONMENT="Production"

echo "=============================================================================="
echo "PROVISIONAMENTO CONCLUÍDO COM SUCESSO!"
echo "Web App URL: https://$WEBAPP_NAME.azurewebsites.net"
echo "Swagger URL: https://$WEBAPP_NAME.azurewebsites.net/swagger"
echo "Oracle Host: $DB_HOST"
echo "User: $APP_DB_USER"
echo "DB Port: 1521"
echo "DB Service Name/PDB: FREEPDB1"
echo "Use as informações acima para configurar seu Pipeline de CD (Release)."
echo "=============================================================================="
