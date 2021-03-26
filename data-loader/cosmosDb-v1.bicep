
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
resource accountName_database_v1 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2020-04-01' = {
  name: '${accountName}/database-v1'
  properties: {
    resource: {
      id: 'database-v1'
    }
    options: {
      autoscaleSettings: {
        maxThroughput: autoscaleMaxThroughput
      }
    }
  }
}

//Resources: add customer container
resource accountName_database_v1_customer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v1.name}/customer'
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

//Resources: add customerAddress container
resource accountName_database_v1_customerAddress 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v1.name}/customerAddress'
  properties: {
    resource: {
      id: 'customerAddress'
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

//Resources: add customerPassword container
resource accountName_database_v1_customerPassword 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v1.name}/customerPassword'
  properties: {
    resource: {
      id: 'customerPassword'
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
resource accountName_database_v1_product 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v1.name}/product'
  properties: {
    resource: {
      id: 'product'
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

//Resources: add productCategory container
resource accountName_database_v1_productCategory 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v1.name}/productCategory'
  properties: {
    resource: {
      id: 'productCategory'
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

//Resources: add productTag container
resource accountName_database_v1_productTag 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v1.name}/productTag'
  properties: {
    resource: {
      id: 'productTag'
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

//Resources: add productTags container
resource accountName_database_v1_productTags 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v1.name}/productTags'
  properties: {
    resource: {
      id: 'productTags'
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

//Resources: add salesOrder container
resource accountName_database_v1_salesOrder 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v1.name}/salesOrder'
  properties: {
    resource: {
      id: 'salesOrder'
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

//Resources: add salesOrderDetail container
resource accountName_database_v1_salesOrderDetail 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v1.name}/salesOrderDetail'
  properties: {
    resource: {
      id: 'salesOrderDetail'
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
