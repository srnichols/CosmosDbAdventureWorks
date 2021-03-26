# az deployment sub create -f ./cosmosMain.bicep -l westus2 -c

<#
New-AzResourceGroup `
 -Name myCosmosAdventureWorks-rg `
 -Location "westus2"

$templateFile = "azuredeploy.json"
$parameterFile="azuredeploy.parameters.json"

New-AzResourceGroupDeployment `
 -Name devenvironment `
 -ResourceGroupName myCosmosAdventureWorks-rg `
 -TemplateFile $templateFile `
 -TemplateParameterFile $parameterFile
#>