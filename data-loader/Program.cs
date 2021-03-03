using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using CosmosDbAdventureWorks.models;
namespace CosmosDbAdventureWorks.data_loader
{
    class Program
    {
        private static IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string uri = config["endpointUri"];
        // The primary key for the Azure Cosmos account.
        private static readonly string key = config["primaryKey"];
        // The region for the Azure Cosmos account.
        private static readonly string region = config["region"];
        // The CosmosDB database througput limit
        private static readonly int throughPut = int.Parse(config["throughPut"]);
        // The path to your data files.
        private static readonly string filePath = config["filePath"];

        // The Cosmos client instance
        private static readonly CosmosClient client = new CosmosClient(uri, key);


        public static async Task Main(string[] args)
        {
            bool exit = false;
            while (exit == false)
            {
                Console.Clear();
                Console.WriteLine($"Load up AdventureWorks sample data into CosmosDB databases");
                Console.WriteLine($"-----------------------------------------");
                Console.WriteLine($"[a]   Load database-v1");
                Console.WriteLine($"[b]   Load database-v2");
                Console.WriteLine($"[c]   Load database-v3");
                Console.WriteLine($"[d]   Load database-v4");
                Console.WriteLine($"[x]   Exit\n");

                ConsoleKeyInfo result = Console.ReadKey(true);

                if (result.KeyChar == 'a')
                {
                    //Console.Clear();
                    Console.WriteLine($"[a]   Load database-v1");
                    await LoadDatabaseV1Async();
                    Console.WriteLine("Database [{0}] fully loaded\n", "database-v1");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }
                else if (result.KeyChar == 'b')
                {
                    //Console.Clear();
                    Console.WriteLine($"[b]   Load database-v2");
                    await LoadDatabaseV2Async();
                    Console.WriteLine("Database [{0}] fully loaded\n", "database-v2");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }
                else if (result.KeyChar == 'c')
                {
                    //Console.Clear();
                    Console.WriteLine($"[c]   Load database-v3");
                    await LoadDatabaseV3Async();
                    Console.WriteLine("Database [{0}] fully loaded\n", "database-v3");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }
                else if (result.KeyChar == 'd')
                {
                    //Console.Clear();
                    Console.WriteLine($"[d]   Load database-v4");
                    await LoadDatabaseV4Async();
                    Console.WriteLine("Database [{0}] fully loaded\n", "database-v4");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }
                else if (result.KeyChar == 'x')
                {
                    exit = true;
                }
            }

        }
        public static async Task LoadDatabaseV1Async()
        {
            // Autoscale throughput settings
            ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(throughPut); //Set autoscale max RU/s

            //Create a new database with autoscale enabled
            Database database = await client.CreateDatabaseIfNotExistsAsync("database-v1", throughputProperties: autoscaleThroughputProperties);
            Console.WriteLine("Created Database: {0}\n", database.Id);

            #region ParallelTasks-LoadDatabaseV1

            // Perform nine tasks in parallel
            Parallel.Invoke(async () =>
                            {
                                await LoadDatabaseV1Customer(database);
                            },  // close 1st Action

                            async () =>
                            {
                                await LoadDatabaseV1CustomerAddress(database);
                            }, //close 2nd Action

                            async () =>
                            {
                                await LoadDatabaseV1CustomerPassword(database);
                            }, //close 3rd Action

                            async () =>
                           {
                                await LoadDatabaseV1Product(database);
                           }, //close 4th Action

                           async () =>
                           {
                                await LoadDatabaseV1ProductCategory(database);
                           }, //close 5th Action

                           async () =>
                           {
                               await LoadDatabaseV1ProductTag(database);
                           }, //close 6th Action

                           async () =>
                           {
                               await LoadDatabaseV1ProductTags(database);
                           }, //close 7th Action

                           async () =>
                           {
                               await LoadDatabaseV1SalesOrder(database);
                           }, //close 8th Action

                           async () =>
                           {
                               await LoadDatabaseV1SalesOrderDetail(database);
                           } //close 9th Action
           ); //close parallel.invoke
            #endregion

        }

        #region LoadDatabaseV1-Containers
        public static async Task LoadDatabaseV1Customer(Database database)
        {
            //*** Customers ***
            // Create a new customer container
            Container containerCustomer = await database.CreateContainerIfNotExistsAsync("customer", "/id");
            Console.WriteLine("Created Container: {0}\n", containerCustomer.Id);
            // Deserialized customer data file
            string jsonStringCustomer = File.ReadAllText(filePath + "cosmic-works-v1/customer.json");
            List<CustomerV1> customers = JsonSerializer.Deserialize<List<CustomerV1>>(jsonStringCustomer);
            Console.WriteLine("Deserialized customer data: {0}\n", customers.Count);
            // Insert customers into the container
            int count = 0;
            foreach (CustomerV1 item in customers)
            {
                ItemResponse<CustomerV1> customerResponse = await containerCustomer.CreateItemAsync<CustomerV1>(item);
                count++;
            }
            Console.WriteLine("{0} customers added to [{1}] container\n", count, containerCustomer.Id);
        }

        public static async Task LoadDatabaseV1CustomerAddress(Database database)
        {
            // *** customerAddress ***
            // Create a new customerAddress container
            Container containerCustomerAddress = await database.CreateContainerIfNotExistsAsync("customerAddress", "/id");
            Console.WriteLine("Created Container: {0}\n", containerCustomerAddress.Id);
            // Deserialized customer data file
            string jsonStringCustomerAddress = File.ReadAllText(filePath + "cosmic-works-v1/customerAddress.json");
            List<CustomerAddress> customerAddresses = JsonSerializer.Deserialize<List<CustomerAddress>>(jsonStringCustomerAddress);
            Console.WriteLine("Deserialized customerAddress data: {0}\n", customerAddresses.Count);
            // Insert customerAddresses into the container
            int count = 0;
            foreach (CustomerAddress item in customerAddresses)
            {
                ItemResponse<CustomerAddress> customerAddressResponse = await containerCustomerAddress.CreateItemAsync<CustomerAddress>(item);
                count++;
            }
            Console.WriteLine("{0} customerAddress added to [{1}] container\n", count, containerCustomerAddress.Id);
        }

        public static async Task LoadDatabaseV1CustomerPassword(Database database)
        {
            // *** customerPassword ***
            // Create a new customerPassword container
            Container containerCustomerPassword = await database.CreateContainerIfNotExistsAsync("customerPassword", "/hash");
            Console.WriteLine("Created Container: {0}\n", containerCustomerPassword.Id);
            // Deserialized customer data file
            string jsonStringCustomerPassword = File.ReadAllText(filePath + "cosmic-works-v1/customerPassword.json");
            List<PasswordV1> customerPasswords = JsonSerializer.Deserialize<List<PasswordV1>>(jsonStringCustomerPassword);
            Console.WriteLine("Deserialized customerPassword data: {0}\n", customerPasswords.Count);
            // Insert customerPassword into the container
            int count = 0;
            foreach (PasswordV1 item in customerPasswords)
            {
                ItemResponse<PasswordV1> customerAddressResponse = await containerCustomerPassword.CreateItemAsync<PasswordV1>(item);
                count++;
            }
            Console.WriteLine("{0} passwords added to [{1}] container\n", count, containerCustomerPassword.Id);
        }

        public static async Task LoadDatabaseV1Product(Database database)
        {
            // *** product ***
            // Create a new products container
            Container containerProduct = await database.CreateContainerIfNotExistsAsync("product", "/id");
            Console.WriteLine("Created Container: {0}\n", containerProduct.Id);
            // Deserialized customer data file
            string jsonStringProducts = File.ReadAllText(filePath + "cosmic-works-v1/product.json");
            List<ProductV1> products = JsonSerializer.Deserialize<List<ProductV1>>(jsonStringProducts);
            Console.WriteLine("Deserialized products data: {0}\n", products.Count);
            // Insert products into the container
            int count = 0;
            foreach (ProductV1 item in products)
            {
                ItemResponse<ProductV1> productsResponse = await containerProduct.CreateItemAsync<ProductV1>(item);
                count++;
            }
            Console.WriteLine("{0} products added to [{1}] container\n", count, containerProduct.Id);
        }

        public static async Task LoadDatabaseV1ProductCategory(Database database)
        {
            // *** productCategory ***
            // Create a new productCategory container
            Container containerProductCategory = await database.CreateContainerIfNotExistsAsync("productCategory", "/id");
            Console.WriteLine("Created Container: {0}\n", containerProductCategory.Id);
            // Deserialized ProductCategory data file
            string jsonStringProductCategory = File.ReadAllText(filePath + "cosmic-works-v1/productCategory.json");
            List<ProductCategoryV1> productCategorys = JsonSerializer.Deserialize<List<ProductCategoryV1>>(jsonStringProductCategory);
            Console.WriteLine("Deserialized productCategory data: {0}\n", productCategorys.Count);
            // Insert productCategory into the container
            int count = 0;
            foreach (ProductCategoryV1 item in productCategorys)
            {
                ItemResponse<ProductCategoryV1> productCategorysResponse = await containerProductCategory.CreateItemAsync<ProductCategoryV1>(item);
                count++;
            }
            Console.WriteLine("{0} productCategory added to [{1}] container\n", count, containerProductCategory.Id);
        }

        public static async Task LoadDatabaseV1ProductTag(Database database)
        {
            // *** producTag ***
            // Create a new productTag container
            Container containerProductTag = await database.CreateContainerIfNotExistsAsync("productTag", "/id");
            Console.WriteLine("Created Container: {0}\n", containerProductTag.Id);
            // Deserialized productTag data file
            string jsonStringProductTag = File.ReadAllText(filePath + "cosmic-works-v1/productTag.json");
            List<Tag> productTag = JsonSerializer.Deserialize<List<Tag>>(jsonStringProductTag);
            Console.WriteLine("Deserialized producTag data: {0}\n", productTag.Count);
            // Insert productTag into the container
            int count = 0;
            foreach (Tag item in productTag)
            {
                ItemResponse<Tag> productTagResponse = await containerProductTag.CreateItemAsync<Tag>(item);
                count++;
            }
            Console.WriteLine("{0} productTag added to [{1}] container\n", count, containerProductTag.Id);
        }

        public static async Task LoadDatabaseV1ProductTags(Database database)
        {
            // *** producTags ***
            // Create a new productTags container
            Container containerProductTags = await database.CreateContainerIfNotExistsAsync("productTags", "/id");
            Console.WriteLine("Created Container: {0}\n", containerProductTags.Id);
            // Deserialized productTag data file
            string jsonStringProductTags = File.ReadAllText(filePath + "cosmic-works-v1/productTags.json");
            List<TagsV1> productTags = JsonSerializer.Deserialize<List<TagsV1>>(jsonStringProductTags);
            Console.WriteLine("Deserialized producTags data: {0}\n", productTags.Count);
            // Insert productTags into the container
            int count = 0;
            foreach (TagsV1 item in productTags)
            {
                ItemResponse<TagsV1> productTagsResponse = await containerProductTags.CreateItemAsync<TagsV1>(item);
                count++;
            }
            Console.WriteLine("{0} productTags added to [{1}] container\n", count, containerProductTags.Id);
        }

        public static async Task LoadDatabaseV1SalesOrder(Database database)
        {
            // *** salesOrder ***
            // Create a new salesOrder container
            Container containerSalesOrder = await database.CreateContainerIfNotExistsAsync("salesOrder", "/id");
            Console.WriteLine("Created Container: {0}\n", containerSalesOrder.Id);
            // Deserialized salesOrder data file
            string jsonStringSalesOrder = File.ReadAllText(filePath + "cosmic-works-v1/salesOrder.json");
            List<SalesOrderV1> salesOrders = JsonSerializer.Deserialize<List<SalesOrderV1>>(jsonStringSalesOrder);
            Console.WriteLine("Deserialized salesOrder data: {0}\n", salesOrders.Count);
            // Insert salesOrder into the container
            int count = 0;
            foreach (SalesOrderV1 item in salesOrders)
            {
                ItemResponse<SalesOrderV1> salesOrdersResponse = await containerSalesOrder.CreateItemAsync<SalesOrderV1>(item);
                count++;
            }
            Console.WriteLine("{0} salesOrders added to [{1}] container\n", count, containerSalesOrder.Id);
        }

        public static async Task LoadDatabaseV1SalesOrderDetail(Database database)
        {
            // *** salesOrderDetail ***
            // Create a new salesOrderDetail container
            Container containerSalesOrderDetail = await database.CreateContainerIfNotExistsAsync("salesOrderDetail", "/id");
            Console.WriteLine("Created Container: {0}\n", containerSalesOrderDetail.Id);
            // Deserialized salesOrderDetail data file
            string jsonStringSalesOrderDetail = File.ReadAllText(filePath + "cosmic-works-v1/salesOrderDetail.json");
            List<SalesOrderDetailV1> salesOrderDetails = JsonSerializer.Deserialize<List<SalesOrderDetailV1>>(jsonStringSalesOrderDetail);
            Console.WriteLine("Deserialized salesOrderDetail data: {0}\n", salesOrderDetails.Count);
            // Insert salesOrderDetail into the container
            int count = 0;
            foreach (SalesOrderDetailV1 item in salesOrderDetails)
            {
                ItemResponse<SalesOrderDetailV1> salesOrdersResponse = await containerSalesOrderDetail.CreateItemAsync<SalesOrderDetailV1>(item);
                count++;
            }
            Console.WriteLine("{0} salesOrderDetails added to [{1}]container\n", count, containerSalesOrderDetail.Id);
        }

        #endregion
        public static async Task LoadDatabaseV2Async()
        {
            // Autoscale throughput settings
            ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(throughPut); //Set autoscale max RU/s

            //Create a new database with autoscale enabled
            Database database = await client.CreateDatabaseIfNotExistsAsync("database-v2", throughputProperties: autoscaleThroughputProperties);
            Console.WriteLine("Created Database: {0}\n", database.Id);

            #region ParallelTasks-LoadDatabaseV2

            // Perform five tasks in parallel
            Parallel.Invoke(async () =>
                             {
                                 await LoadDatabaseV2Customer(database);
                             },  // close first Action

                             async () =>
                             {
                                 await LoadDatabaseV2Product(database);
                             }, //close second Action

                             async () =>
                             {
                                 await LoadDatabaseV2ProductCategory(database);
                             }, //close third Action
                             async () =>
                             {
                                 await LoadDatabaseV2ProductTag(database);
                             }, //close forth Action
                             async () =>
                             {
                                 await LoadDatabaseV2SalesOrder(database);
                             } //close fith Action
                         ); //close parallel.invoke
            #endregion

            Console.WriteLine("Database [{0}] fully loaded\n", database.Id);
        }

        #region LoadDatabaseV2-Containers
        public static async Task LoadDatabaseV2Customer(Database database)
        {
            //*** Customers ***
            // Create a new customer container
            Container containerCustomer = await database.CreateContainerIfNotExistsAsync("customer", "/id");
            Console.WriteLine("Created Container: {0}\n", containerCustomer.Id);
            // Deserialized customer data file
            string jsonStringCustomer = File.ReadAllText(filePath + "cosmic-works-v2/customer.json");
            List<CustomerV2> customers = JsonSerializer.Deserialize<List<CustomerV2>>(jsonStringCustomer);
            Console.WriteLine("Deserialized customer data: {0}\n", customers.Count);
            // Insert customers into the container
            int count = 0;
            foreach (CustomerV2 item in customers)
            {
                ItemResponse<CustomerV2> customerResponse = await containerCustomer.CreateItemAsync<CustomerV2>(item);
                count++;
            }
            Console.WriteLine("{0} customers added to [{1}] container\n", count, containerCustomer.Id);
        }

        public static async Task LoadDatabaseV2Product(Database database)
        {
            // *** product ***
            // Create a new products container
            Container containerProduct = await database.CreateContainerIfNotExistsAsync("product", "/categoryId");
            Console.WriteLine("Created Container: {0}\n", containerProduct.Id);
            // Deserialized customer data file
            string jsonStringProducts = File.ReadAllText(filePath + "cosmic-works-v2/product.json");
            List<Product> products = JsonSerializer.Deserialize<List<Product>>(jsonStringProducts);
            Console.WriteLine("Deserialized products data: {0}\n", products.Count);
            // Insert products into the container
            int count = 0;
            foreach (Product item in products)
            {
                ItemResponse<Product> productsResponse = await containerProduct.CreateItemAsync<Product>(item);
                count++;
            }
            Console.WriteLine("{0} products added to [{1}] container\n", count, containerProduct.Id);
        }

        public static async Task LoadDatabaseV2ProductCategory(Database database)
        {
            // *** productCategory ***
            // Create a new productCategory container
            Container containerProductCategory = await database.CreateContainerIfNotExistsAsync("productCategory", "/type");
            Console.WriteLine("Created Container: {0}\n", containerProductCategory.Id);
            // Deserialized ProductCategory data file
            string jsonStringProductCategory = File.ReadAllText(filePath + "cosmic-works-v2/productCategory.json");
            List<ProductCategory> productCategorys = JsonSerializer.Deserialize<List<ProductCategory>>(jsonStringProductCategory);
            Console.WriteLine("Deserialized productCategory data: {0}\n", productCategorys.Count);
            // Insert productCategory into the container
            int count = 0;
            foreach (ProductCategory item in productCategorys)
            {
                ItemResponse<ProductCategory> productCategorysResponse = await containerProductCategory.CreateItemAsync<ProductCategory>(item);
                count++;
            }
            Console.WriteLine("{0} productCategory added to [{1}] container\n", count, containerProductCategory.Id);
        }

        public static async Task LoadDatabaseV2ProductTag(Database database)
        {
            // *** producTag ***
            // Create a new productTag container
            Container containerProductTag = await database.CreateContainerIfNotExistsAsync("productTag", "/type");
            Console.WriteLine("Created Container: {0}\n", containerProductTag.Id);
            // Deserialized productTag data file
            string jsonStringProductTag = File.ReadAllText(filePath + "cosmic-works-v2/productTag.json");
            List<TagV2> productTag = JsonSerializer.Deserialize<List<TagV2>>(jsonStringProductTag);
            Console.WriteLine("Deserialized producTag data: {0}\n", productTag.Count);
            // Insert productTag into the container
            int count = 0;
            foreach (TagV2 item in productTag)
            {
                ItemResponse<TagV2> productTagResponse = await containerProductTag.CreateItemAsync<TagV2>(item);
                count++;
            }
            Console.WriteLine("{0} productTag added to [{1}] container\n", count, containerProductTag.Id);
        }

        public static async Task LoadDatabaseV2SalesOrder(Database database)
        {
            // *** salesOrder ***
            // Create a new salesOrder container
            Container containerSalesOrder = await database.CreateContainerIfNotExistsAsync("salesOrder", "/customerId");
            Console.WriteLine("Created Container: {0}\n", containerSalesOrder.Id);
            // Deserialized salesOrder data file
            string jsonStringSalesOrder = File.ReadAllText(filePath + "cosmic-works-v2/salesOrder.json");
            List<SalesOrder> salesOrders = JsonSerializer.Deserialize<List<SalesOrder>>(jsonStringSalesOrder);
            Console.WriteLine("Deserialized salesOrder data: {0}\n", salesOrders.Count);
            // Insert salesOrder into the container
            int count = 0;
            foreach (SalesOrder item in salesOrders)
            {
                ItemResponse<SalesOrder> salesOrdersResponse = await containerSalesOrder.CreateItemAsync<SalesOrder>(item);
                count++;
            }
            Console.WriteLine("{0} salesOrders added to [{1}] container\n", count, containerSalesOrder.Id);
        }
        #endregion

        public static async Task LoadDatabaseV3Async()
        {
            // Autoscale throughput settings
            ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(throughPut); //Set autoscale max RU/s

            //Create a new database with autoscale enabled
            Database database = await client.CreateDatabaseIfNotExistsAsync("database-v3", throughputProperties: autoscaleThroughputProperties);
            Console.WriteLine("Created Database: {0}\n", database.Id);

            #region ParallelTasks-LoadDatabaseV3

            // Perform five tasks in parallel
            Parallel.Invoke(async () =>
                            {
                                await LoadDatabaseV3Customer(database);
                            },  // close 1st Action

                             async () =>
                             {
                                 await LoadDatabaseV3Product(database);
                             }, //close 2nd Action

                             async () =>
                             {
                                 await LoadDatabaseV3ProductCategory(database);
                             }, //close 3rd Action
                             async () =>
                             {
                                 await LoadDatabaseV3ProductTag(database);
                             }, //close 4th Action
                             async () =>
                             {
                                 await LoadDatabaseV3SalesOrder(database);
                             } //close 5th Action
                         ); //close parallel.invoke
            #endregion

            Console.WriteLine("Database [{0}] fully loaded\n", database.Id);
        }

        #region LoadDatabaseV3-Containers
        public static async Task LoadDatabaseV3Customer(Database database)
        {
            //*** Customers ***
            // Create a new customer container
            Container containerCustomer = await database.CreateContainerIfNotExistsAsync("customer", "/id");
            Console.WriteLine("Created Container: {0}\n", containerCustomer.Id);
            // Deserialized customer data file
            string jsonStringCustomer = File.ReadAllText(filePath + "cosmic-works-v3/customer.json");
            List<CustomerV2> customers = JsonSerializer.Deserialize<List<CustomerV2>>(jsonStringCustomer);
            Console.WriteLine("Deserialized customer data: {0}\n", customers.Count);
            // Insert customers into the container
            int count = 0;
            foreach (CustomerV2 item in customers)
            {
                ItemResponse<CustomerV2> customerResponse = await containerCustomer.CreateItemAsync<CustomerV2>(item);
                count++;
            }
            Console.WriteLine("{0} customers added to [{1}] container\n", count, containerCustomer.Id);
        }

        public static async Task LoadDatabaseV3Product(Database database)
        {
            // *** product ***
            // Create a new products container
            Container containerProduct = await database.CreateContainerIfNotExistsAsync("product", "/categoryId");
            Console.WriteLine("Created Container: {0}\n", containerProduct.Id);
            // Deserialized customer data file
            string jsonStringProducts = File.ReadAllText(filePath + "cosmic-works-v3/product.json");
            List<Product> products = JsonSerializer.Deserialize<List<Product>>(jsonStringProducts);
            Console.WriteLine("Deserialized products data: {0}\n", products.Count);
            // Insert products into the container
            int count = 0;
            foreach (Product item in products)
            {
                ItemResponse<Product> productsResponse = await containerProduct.CreateItemAsync<Product>(item);
                count++;
            }
            Console.WriteLine("{0} products added to [{1}] container\n", count, containerProduct.Id);
        }

        public static async Task LoadDatabaseV3ProductCategory(Database database)
        {
            // *** productCategory ***
            // Create a new productCategory container
            Container containerProductCategory = await database.CreateContainerIfNotExistsAsync("productCategory", "/type");
            Console.WriteLine("Created Container: {0}\n", containerProductCategory.Id);
            // Deserialized ProductCategory data file
            string jsonStringProductCategory = File.ReadAllText(filePath + "cosmic-works-v3/productCategory.json");
            List<ProductCategory> productCategorys = JsonSerializer.Deserialize<List<ProductCategory>>(jsonStringProductCategory);
            Console.WriteLine("Deserialized productCategory data: {0}\n", productCategorys.Count);
            // Insert productCategory into the container
            int count = 0;
            foreach (ProductCategory item in productCategorys)
            {
                ItemResponse<ProductCategory> productCategorysResponse = await containerProductCategory.CreateItemAsync<ProductCategory>(item);
                count++;
            }
            Console.WriteLine("{0} productCategory added to [{1}] container\n", count, containerProductCategory.Id);
        }

        public static async Task LoadDatabaseV3ProductTag(Database database)
        {
            // *** producTag ***
            // Create a new productTag container
            Container containerProductTag = await database.CreateContainerIfNotExistsAsync("productTag", "/type");
            Console.WriteLine("Created Container: {0}\n", containerProductTag.Id);
            // Deserialized productTag data file
            string jsonStringProductTag = File.ReadAllText(filePath + "cosmic-works-v3/productTag.json");
            List<TagV2> productTag = JsonSerializer.Deserialize<List<TagV2>>(jsonStringProductTag);
            Console.WriteLine("Deserialized producTag data: {0}\n", productTag.Count);
            // Insert productTag into the container
            int count = 0;
            foreach (TagV2 item in productTag)
            {
                ItemResponse<TagV2> productTagResponse = await containerProductTag.CreateItemAsync<TagV2>(item);
                count++;
            }
            Console.WriteLine("{0} productTag added to [{1}] container\n", count, containerProductTag.Id);
        }

        public static async Task LoadDatabaseV3SalesOrder(Database database)
        {
            // *** salesOrder ***
            // Create a new salesOrder container
            Container containerSalesOrder = await database.CreateContainerIfNotExistsAsync("salesOrder", "/customerId");
            Console.WriteLine("Created Container: {0}\n", containerSalesOrder.Id);
            // Deserialized salesOrder data file
            string jsonStringSalesOrder = File.ReadAllText(filePath + "cosmic-works-v3/salesOrder.json");
            List<SalesOrder> salesOrders = JsonSerializer.Deserialize<List<SalesOrder>>(jsonStringSalesOrder);
            Console.WriteLine("Deserialized salesOrder data: {0}\n", salesOrders.Count);
            // Insert salesOrder into the container
            int count = 0;
            foreach (SalesOrder item in salesOrders)
            {
                ItemResponse<SalesOrder> salesOrdersResponse = await containerSalesOrder.CreateItemAsync<SalesOrder>(item);
                count++;
            }
            Console.WriteLine("{0} salesOrders added to [{1}] container\n", count, containerSalesOrder.Id);
        }
        #endregion

        public static async Task LoadDatabaseV4Async()
        {
            // Autoscale throughput settings
            ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(throughPut); //Set autoscale max RU/s

            //Create a new database with autoscale enabled
            Database database = await client.CreateDatabaseIfNotExistsAsync("database-v4", throughputProperties: autoscaleThroughputProperties);
            Console.WriteLine("Created Database: {0}\n", database.Id);

            #region ParallelTasks-LoadDatabaseV4

            // Perform five tasks in parallel
            Parallel.Invoke(async () =>
                            {
                                await LoadDatabaseV4Customer(database);
                            },  // close 1st Action

                             async () =>
                             {
                                 await LoadDatabaseV4Product(database);
                             }, //close 2nd Action

                             async () =>
                             {
                                 await LoadDatabaseV4ProductMeta(database);
                             }, //close 3rd Action
                             async () =>
                             {
                                 await LoadDatabaseV4SalesByCategory(database);
                             } //close 4th Action
                         ); //close parallel.invoke
            #endregion

            Console.WriteLine("Database [{0}] fully loaded\n", database.Id);
        }

        #region LoadDatabaseV4-Containers
        public static async Task LoadDatabaseV4Customer(Database database)
        {
            //*** Customers ***
            // Create a new customer container
            Container containerCustomer = await database.CreateContainerIfNotExistsAsync("customer", "/id");
            Console.WriteLine("Created Container: {0}\n", containerCustomer.Id);
            // Deserialized customer data file
            string jsonStringCustomer = File.ReadAllText(filePath + "cosmic-works-v4/customer.json");
            List<CustomerV4> customers = JsonSerializer.Deserialize<List<CustomerV4>>(jsonStringCustomer);
            Console.WriteLine("Deserialized customer data: {0}\n", customers.Count);
            // Insert customers into the container
            int count = 0;
            foreach (CustomerV4 item in customers)
            {
                ItemResponse<CustomerV4> customerResponse = await containerCustomer.CreateItemAsync<CustomerV4>(item);
                count++;
            }
            Console.WriteLine("{0} customers added to [{1}] container\n", count, containerCustomer.Id);
        }

        public static async Task LoadDatabaseV4Product(Database database)
        {
            // *** product ***
            // Create a new products container
            Container containerProduct = await database.CreateContainerIfNotExistsAsync("product", "/categoryId");
            Console.WriteLine("Created Container: {0}\n", containerProduct.Id);
            // Deserialized customer data file
            string jsonStringProducts = File.ReadAllText(filePath + "cosmic-works-v4/product.json");
            List<Product> products = JsonSerializer.Deserialize<List<Product>>(jsonStringProducts);
            Console.WriteLine("Deserialized products data: {0}\n", products.Count);
            // Insert products into the container
            int count = 0;
            foreach (Product item in products)
            {
                ItemResponse<Product> productsResponse = await containerProduct.CreateItemAsync<Product>(item);
                count++;
            }
            Console.WriteLine("{0} products added to [{1}] container\n", count, containerProduct.Id);
        }

        public static async Task LoadDatabaseV4ProductMeta(Database database)
        {
            // *** productMeta ***
            // Create a new productCategory container
            Container containerProductMeta = await database.CreateContainerIfNotExistsAsync("productMeta", "/type");
            Console.WriteLine("Created Container: {0}\n", containerProductMeta.Id);
            // Deserialized productMeta data file
            string jsonStringProductCategory = File.ReadAllText(filePath + "cosmic-works-v4/productMeta.json");
            List<ProductMeta> productMetas = JsonSerializer.Deserialize<List<ProductMeta>>(jsonStringProductCategory);
            Console.WriteLine("Deserialized productMeta data: {0}\n", productMetas.Count);
            // Insert productMeta into the container
            int count = 0;
            foreach (ProductMeta item in productMetas)
            {
                ItemResponse<ProductMeta> productMetasResponse = await containerProductMeta.CreateItemAsync<ProductMeta>(item);
                count++;
            }
            Console.WriteLine("{0} productMeta added to [{1}] container\n", count, containerProductMeta.Id);
        }

        public static async Task LoadDatabaseV4SalesByCategory(Database database)
        {
            // *** salesByCategory ***
            // Create a new salesByCategory container
            Container containerSalesByCategory = await database.CreateContainerIfNotExistsAsync("salesByCategory", "/categoryId");
            Console.WriteLine("Created Container: {0}\n", containerSalesByCategory.Id);

            // Data gets loaded into salesByCategory container by a change feed process

        }
        #endregion
    }
}
