
$templateFile = "azuredeploy.json"
$parameterFile="azuredeploy.parameters.json"
New-AzResourceGroup `
  -Name myCosmicWork-rg `
  -Location "westus2"
New-AzResourceGroupDeployment `
  -Name devenvironment `
  -ResourceGroupName myCosmicWork-rg `
  -TemplateFile $templateFile `
  -TemplateParameterFile $parameterFile