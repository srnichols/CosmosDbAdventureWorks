//Parameters  
@minLength(6)
@maxLength(44)
@description('Cosmos DB account name, max length 44 characters, lowercase')
param accountName string 

@minValue(4000)
@maxValue(100000)
@description('Maximum throughput when using Autoscale Throughput Policy for the database')
param autoscaleMaxThroughput int = 4000

//Resources: setup database
resource accountName_database_v4 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2020-04-01' = {
  name: '${accountName}/database-v4'
  properties: {
    resource: {
      id: 'database-v4'
    }
    options: {
      autoscaleSettings: {
        maxThroughput: autoscaleMaxThroughput
      }
    }
  }
}

//Resources: add customer container
resource accountName_database_v4_customer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v4.name}/customer'
  properties: {
    resource: {
      id: 'customer'
      partitionKey: {
        paths: [
          '/customerId'
        ]
        kind: 'Hash'
      }
    }
    options: {}
  }
}

//Resources: add product container
resource accountName_database_v4_product 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v4.name}/product'
  properties: {
    resource: {
      id: 'product'
      partitionKey: {
        paths: [
          '/categoryId'
        ]
        kind: 'Hash'
      }
    }
    options: {}
  }
}

//Resources: add productMeta container
resource accountName_database_v4_productMeta 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v4.name}/productMeta'
  properties: {
    resource: {
      id: 'productMeta'
      partitionKey: {
        paths: [
          '/type'
        ]
        kind: 'Hash'
      }
    }
    options: {}
  }
}

//Resources: add salesByCategory container
resource accountName_database_v4_salesByCategory 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v4.name}/salesByCategory'
  properties: {
    resource: {
      id: 'salesByCategory'
      partitionKey: {
        paths: [
          '/categoryId'
        ]
        kind: 'Hash'
      }
    }
    options: {}
  }
}
