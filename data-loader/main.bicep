param accountName string {
  metadata: {
    description: 'Cosmos DB account name, max length 44 characters, lowercase'
  }
  default: 'cosmicworks-${uniqueString(resourceGroup().id)}'
}
param location string {
  metadata: {
    description: 'Location for the Cosmos DB account.'
  }
  default: resourceGroup().location
}
param autoscaleMaxThroughput int {
  minValue: 4000
  maxValue: 1000000
  metadata: {
    description: 'Maximum throughput when using Autoscale Throughput Policy for the database'
  }
  default: 4000
}

resource accountName_resource 'Microsoft.DocumentDB/databaseAccounts@2020-04-01' = {
  name: accountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
  }
}

resource accountName_database_v1 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2020-04-01' = {
  name: '${accountName_resource.name}/database-v1'
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

resource accountName_database_v2 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2020-04-01' = {
  name: '${accountName_resource.name}/database-v2'
  properties: {
    resource: {
      id: 'database-v2'
    }
    options: {
      autoscaleSettings: {
        maxThroughput: autoscaleMaxThroughput
      }
    }
  }
}

resource accountName_database_v3 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2020-04-01' = {
  name: '${accountName_resource.name}/database-v3'
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

resource accountName_database_v4 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2020-04-01' = {
  name: '${accountName_resource.name}/database-v4'
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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

resource accountName_database_v2_customer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v2.name}/customer'
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
  dependsOn: [
    accountName_resource
  ]
}

resource accountName_database_v2_product 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v2.name}/product'
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
  dependsOn: [
    accountName_resource
  ]
}

resource accountName_database_v2_productCategory 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v2.name}/productCategory'
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
  dependsOn: [
    accountName_resource
  ]
}

resource accountName_database_v2_productTag 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v2.name}/productTag'
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
  dependsOn: [
    accountName_resource
  ]
}

resource accountName_database_v2_salesOrder 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2020-04-01' = {
  name: '${accountName_database_v2.name}/salesOrder'
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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}

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
  dependsOn: [
    accountName_resource
  ]
}