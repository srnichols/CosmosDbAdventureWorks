
$templateFile = "azuredeploy.json"
$parameterFile="azuredeploy.parameters.json"
New-AzResourceGroup `
  -Name myCosmosAdventureWorks-rg `
  -Location "westus2"
New-AzResourceGroupDeployment `
  -Name devenvironment `
  -ResourceGroupName myCosmosAdventureWorks-rg `
  -TemplateFile $templateFile `
  -TemplateParameterFile $parameterFile