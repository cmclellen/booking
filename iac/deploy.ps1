

$ENVIRONMENT="prd"
$RESOURCE_GROUP_NAME="Reservation"
$PARAM_FILE="$ENVIRONMENT.bicepparam"
$DEPLOYMENT_NAME="deployment-$($RESOURCE_GROUP_NAME.ToLower())-$ENVIRONMENT"
# az login --tenant dca5775e-99b4-497c-90c1-c8e73396999e // Mine
az deployment group create --name $DEPLOYMENT_NAME --resource-group $RESOURCE_GROUP_NAME --template-file main.bicep --parameters $PARAM_FILE