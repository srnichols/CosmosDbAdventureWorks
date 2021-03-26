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
resource accountName_database_v3 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2020-04-01' = {
  name: '${accountName}/database-v3'
  properties: {
    resource: {
      id: 'database-v3'
    }
    options: {
      autoscaleSettings: {
        maxThroughput: autoscaleMaxThroughput
      }
    }
  }
}

//Resources: add customer container
resource accountName_database_v3_customer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v3.name}/customer'
  properties: {
    resource: {
      id: 'customer'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
    }
    options: {}
  }
}

//Resources: add product container
resource accountName_database_v3_product 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v3.name}/product'
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

//Resources: add productCategory container
resource accountName_database_v3_productCategory 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v3.name}/productCategory'
  properties: {
    resource: {
      id: 'productCategory'
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

//Resources: add productTag container
resource accountName_database_v3_productTag 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v3.name}/productTag'
  properties: {
    resource: {
      id: 'productTag'
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

//Resources: add salesOrder container
resource accountName_database_v3_salesOrder 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v3.name}/salesOrder'
  properties: {
    resource: {
      id: 'salesOrder'
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
