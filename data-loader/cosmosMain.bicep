//Top Scoping
targetScope = 'subscription'

//Parameters
@minLength(3)
@maxLength(64)
@description('Your ResouceGroup name, max length 64 characters, mixedCase')
param rgName string = 'myCosmosAdventureWorks-rg2'


@minLength(6)
@maxLength(44)
@description('Cosmos DB account name, max length 44 characters, lowercase')
param accountName string = 'srn-csa-cosmosadventureworks2'

@minValue(4000)
@maxValue(100000)
@description('Maximum throughput when using Autoscale Throughput Policy for the database')
param autoscaleMaxThroughput int = 4000

//Set ResourceGroup 
resource rg 'Microsoft.Resources/resourceGroups@2020-06-01' = {
 name: rgName
 location: deployment().location
}

//Load moduel: cosmosDB Account setup 
module cosmosAccountDeploy './cosmosAccount.bicep' = {
  name: 'cosmosAccountDeploy'
  scope: rg
  params:{
     accountName: accountName
  }
}

//Load moduel: cosmosDB-v1 database & container setup 
module cosmosDatabaseV1 './cosmosDb-v1.bicep' = {
  name: 'cosmosDatabaseV1'
  scope: rg
  params:{
     accountName: accountName
     autoscaleMaxThroughput: autoscaleMaxThroughput  
  }
} 

//Load moduel: cosmosDB-v2 database & container setup 
module cosmosDatabaseV2 './cosmosDb-v2.bicep' = {
  name: 'cosmosDatabaseV2'
  scope: rg
  params:{
     accountName: accountName
     autoscaleMaxThroughput: autoscaleMaxThroughput  
  }
}

//Load moduel: cosmosDB-v3 database & container setup 
module cosmosDatabaseV3 './cosmosDb-v3.bicep' = {
  name: 'cosmosDatabaseV3'
  scope: rg
  params:{
     accountName: accountName
     autoscaleMaxThroughput: autoscaleMaxThroughput  
  }
}

//Load moduel: cosmosDB-v4 database & container setup 
module cosmosDatabaseV4 './cosmosDb-v4.bicep' = {
  name: 'cosmosDatabaseV4'
  scope: rg
  params:{
     accountName: accountName
     autoscaleMaxThroughput: autoscaleMaxThroughput  
  }
}





