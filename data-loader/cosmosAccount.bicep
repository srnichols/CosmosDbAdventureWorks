//Parameters  
@minLength(6)
@maxLength(44)
@description('Cosmos DB account name, max length 44 characters, lowercase')
param accountName string = 'cosmosadventureworks-${uniqueString(resourceGroup().id)}'


//Resources
resource accountName_resource  'Microsoft.DocumentDB/databaseAccounts@2020-04-01' = {
  name: accountName
  location: resourceGroup().location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: resourceGroup().location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
  }
}

//Output
output accountId string = accountName_resource.id
