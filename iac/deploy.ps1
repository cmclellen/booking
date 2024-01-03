

$ENVIRONMENT="prd"
$RESOURCE_GROUP_NAME="Reservation"
$PARAM_FILE="$ENVIRONMENT.bicepparam"
$DEPLOYMENT_NAME="deployment-$($RESOURCE_GROUP_NAME.ToLower())-$ENVIRONMENT"
$LOCATION="australiaeast"
$TEMPLATE_FILE="main.bicep"

Get-AzResourceGroup -Name $RESOURCE_GROUP_NAME -ErrorVariable notPresent -ErrorAction SilentlyContinue
if ($notPresent)
{
    New-AzResourceGroup -Name $RESOURCE_GROUP_NAME -Location $LOCATION -Tag @{Empty=$null; Environment=$ENVIRONMENT}
}

az bicep build-params --file $PARAM_FILE
New-AzResourceGroupDeployment -Name $DEPLOYMENT_NAME -ResourceGroupName $RESOURCE_GROUP_NAME -TemplateFile $TEMPLATE_FILE -TemplateParameterFile "$ENVIRONMENT.json"

# az login --tenant dca5775e-99b4-497c-90c1-c8e73396999e
# connect-azaccount -Tenant dca5775e-99b4-497c-90c1-c8e73396999e
# get-azlocation | where-object {$_.location -Like "australia*"} | select-object -Property location
# Create service principal for the identity
# az ad sp create-for-rbac --name GHReservation --role owner --scope /subscriptions/761399c5-3790-4380-b6a8-a11554fafa7a/resourceGroups/Reservation