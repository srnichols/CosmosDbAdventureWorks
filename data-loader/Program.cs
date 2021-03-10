using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Diagnostics;
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
        // The CosmosDB database througput limit
        private static readonly bool serverless = bool.Parse(config["serverless"]);

        private static readonly bool bulkExecution = bool.Parse(config["bulkExecution"]);
        
        // The path to your data files.
        private static readonly string filePath = config["filePath"];

        // The Cosmos client instance
        private static readonly CosmosClient client = new CosmosClient(uri, key, new CosmosClientOptions() { AllowBulkExecution = bulkExecution });

        public static async Task Main(string[] args)
        {
            bool exit = false;
            while (exit == false)
            {
                Console.Clear();
                Console.WriteLine($"Load up AdventureWorks sample data into CosmosDB containers");
                Console.WriteLine($"Item Loads = Is a slower safe trickle for non-prod DBs");
                Console.WriteLine($"Bulk Loads = Only works on high performance prod DBs");
                Console.WriteLine($"-----------------------------------------------------------");
                Console.WriteLine($"[a]   Item Load database-v1");
                Console.WriteLine($"[b]   Item Load database-v2");
                Console.WriteLine($"[c]   Item Load database-v3");
                Console.WriteLine($"[d]   Item Load database-v4");
                Console.WriteLine($"[e]   Bulk Load database-v1");
                Console.WriteLine($"[f]   Bulk Load database-v2");
                Console.WriteLine($"[g]   Bulk Load database-v3");
                Console.WriteLine($"[h]   Bulk Load database-v4");
                Console.WriteLine($"[x]   Exit\n");

                ConsoleKeyInfo result = Console.ReadKey(true);

                if (result.KeyChar == 'a')
                {
                    //Console.Clear();
                    Console.WriteLine($"[a]   Item Load database-v1 [Selected]\n");
                    await LoadDatabaseV1Async();
                }
                else if (result.KeyChar == 'b')
                {
                    //Console.Clear();
                    Console.WriteLine($"[b]   Item Load database-v2 [Selected]\n");
                    await LoadDatabaseV2Async();
                }
                else if (result.KeyChar == 'c')
                {
                    //Console.Clear();
                    Console.WriteLine($"[c]   Item Load database-v3 [Selected]\n");
                    await LoadDatabaseV3Async();
                }
                else if (result.KeyChar == 'd')
                {
                    //Console.Clear();
                    Console.WriteLine($"[d]   Item Load database-v4 [Selected]\n");
                    await LoadDatabaseV4Async();
                }
                else if (result.KeyChar == 'e')
                {
                    //Console.Clear();
                    Console.WriteLine($"[e]   Bulk Load database-v1 [Selected]\n");
                    await BulkLoadDatabaseV1Async();
                }
                else if (result.KeyChar == 'f')
                {
                    //Console.Clear();
                    Console.WriteLine($"[f]   Bulk Load database-v2 [Selected]\n");
                    await BulkLoadDatabaseV2Async();
                }
                else if (result.KeyChar == 'g')
                {
                    //Console.Clear();
                    Console.WriteLine($"[g]   Bulk Load database-v3 [Selected]\n");
                    await BulkLoadDatabaseV3Async();
                }
                else if (result.KeyChar == 'h')
                {
                    //Console.Clear();
                    Console.WriteLine($"[h]   Bulk Load database-v4 [Selected]\n");
                    await BulkLoadDatabaseV4Async();
                }
                else if (result.KeyChar == 'x')
                {
                    exit = true;
                }
            }
        }

        public static async Task LoadDatabaseV1Async()
        {
            try
            { 
                Database database = null;

                if (serverless)
                {
                    //Create a new database using serverless configuration 
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v1");
                }
                else
                { 
                    // Autoscale throughput settings
                    ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(throughPut); //Set autoscale max RU/s

                    //Create a new database with autoscale enabled
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v1", throughputProperties: autoscaleThroughputProperties);
                    Console.WriteLine("Created Database: {0}\n", database.Id);
                }

                #region ParallelTasks-LoadDatabaseV1

                // Perform nine tasks in parallel
                Stopwatch stopwatch = Stopwatch.StartNew();
                Task t1 = LoadDatabaseV1Customer(database);
                Task t2 = LoadDatabaseV1CustomerAddress(database);
                Task t3 = LoadDatabaseV1CustomerPassword(database);
                Task t4 = LoadDatabaseV1Product(database);
                Task t5 = LoadDatabaseV1ProductCategory(database);
                Task t6 = LoadDatabaseV1ProductTag(database);
                Task t7 = LoadDatabaseV1ProductTags(database);
                Task t8 = LoadDatabaseV1SalesOrder(database);
                Task t9 = LoadDatabaseV1SalesOrderDetail(database);
                
                await Task.WhenAll(t1, t2, t3, t4,t5,t6,t7,t8,t9);
                stopwatch.Stop();
                Console.WriteLine("All database-v1 containers loaded in {0}\npress any key to exit.\n",stopwatch.Elapsed);

                #endregion

            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.ReadKey();
            }
        }

        #region LoadDatabaseV1-Containers
        public static async Task LoadDatabaseV1Customer(Database database)
        {
            //*** Customers ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("customer", "/id");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/customer.json");
            List<CustomerV1> sourceItemList = JsonSerializer.Deserialize<List<CustomerV1>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();

                // Lets multi-thread all the inserts 
                var tasks = new Task[sourceItemList.Count];
                var taskCount = 0;
                foreach (CustomerV1 item in sourceItemList)
                {
                    tasks[taskCount] = containerDestination
                        .CreateItemAsync(item, new PartitionKey(item.id));
                    taskCount++;
                }
                await Task.WhenAll(tasks);
            
            /*
            // Lets insert one record at a time
            int count = 0;
            foreach (CustomerV1 item in sourceItemList)
            {
                ItemResponse<CustomerV1> customerResponse = await containerDestination.CreateItemAsync<CustomerV1>(item);
                count++;
            }
            */
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV1CustomerAddress(Database database)
        {
            // *** customerAddress ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("customerAddress", "/id");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/customerAddress.json");
            List<CustomerAddress> sourceItemList = JsonSerializer.Deserialize<List<CustomerAddress>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Lets multi-thread all the inserts 
            var tasks = new Task[sourceItemList.Count];
            var taskCount = 0;
            foreach (CustomerAddress item in sourceItemList)
            {
                tasks[taskCount] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.id));
                taskCount++;
            }
            await Task.WhenAll(tasks);

            /*
            // Lets insert one record at a time
            int count = 0;
            foreach (CustomerAddress item in sourceItemList)
            {
               ItemResponse<CustomerAddress> customerResponse = await containerDestination.CreateItemAsync<CustomerAddress>(item);
               count++;
            }
            */
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV1CustomerPassword(Database database)
        {
            // *** customerPassword ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("customerPassword", "/hash");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/customerPassword.json");
            List<PasswordV1> sourceItemList = JsonSerializer.Deserialize<List<PasswordV1>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Lets multi-thread all the inserts 
            var tasks = new Task[sourceItemList.Count];
            var taskCount = 0;
            foreach (PasswordV1 item in sourceItemList)
            {
                tasks[taskCount] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.hash));
                taskCount++;
            }
            await Task.WhenAll(tasks);

            /*
            // Lets insert one record at a time            
            int count = 0;
            foreach (PasswordV1 item in sourceItemList)
            {
                ItemResponse<PasswordV1> customerResponse = await containerDestination.CreateItemAsync<PasswordV1>(item);
                count++;
            }
            */
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV1Product(Database database)
        {
            // *** product ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("product", "/id");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/product.json");
            List<ProductV1> sourceItemList = JsonSerializer.Deserialize<List<ProductV1>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();
            /*
            // Lets multi-thread all the inserts 
            var tasks = new Task[sourceItemList.Count];
            var taskCount = 0;
            foreach (ProductV1 item in sourceItemList)
            {
                tasks[taskCount] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.id));
                taskCount++;
            }
            await Task.WhenAll(tasks);
            */
            
            // Lets insert one record at a time  
            int count = 0;
            foreach (ProductV1 item in sourceItemList)
            {
                ItemResponse<ProductV1> customerResponse = await containerDestination.CreateItemAsync<ProductV1>(item);
                count++;
            }
            
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV1ProductCategory(Database database)
        {
            // *** productCategory ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("productCategory", "/id");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/productCategory.json");
            List<ProductCategoryV1> sourceItemList = JsonSerializer.Deserialize<List<ProductCategoryV1>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();
            /*
            // Lets multi-thread all the inserts 
            var tasks = new Task[sourceItemList.Count];
            var taskCount = 0;
            foreach (ProductCategoryV1 item in sourceItemList)
            {
                tasks[taskCount] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.id));
                taskCount++;
            }
            await Task.WhenAll(tasks);
            */
            
            // Lets insert one record at a time 
            int count = 0;
            foreach (ProductCategoryV1 item in sourceItemList)
            {
                ItemResponse<ProductCategoryV1> customerResponse = await containerDestination.CreateItemAsync<ProductCategoryV1>(item);
                count++;
            }
            
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV1ProductTag(Database database)
        {
            // *** producTag ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("productTag", "/id");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/productTag.json");
            List<Tag> sourceItemList = JsonSerializer.Deserialize<List<Tag>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();
            /*
            // Lets multi-thread all the inserts 
            var tasks = new Task[sourceItemList.Count];
            var taskCount = 0;
            foreach (Tag item in sourceItemList)
            {
                tasks[taskCount] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.id));
                taskCount++;
            }
            await Task.WhenAll(tasks);
            */
            
            // Lets insert one record at a time
            int count = 0;
            foreach (Tag item in sourceItemList)
            {
                ItemResponse<Tag> customerResponse = await containerDestination.CreateItemAsync<Tag>(item);
                count++;
            }
            
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV1ProductTags(Database database)
        {
            // *** producTags ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("productTags", "/id");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/productTags.json");
            List<TagsV1> sourceItemList = JsonSerializer.Deserialize<List<TagsV1>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();
            /*
            // Lets multi-thread all the inserts 
            var tasks = new Task[sourceItemList.Count];
            var taskCount = 0;
            foreach (TagsV1 item in sourceItemList)
            {
                tasks[taskCount] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.id));
                taskCount++;
            }
            await Task.WhenAll(tasks);
            */
            
            // Lets insert one record at a time
            int count = 0;
            foreach (TagsV1 item in sourceItemList)
            {
                ItemResponse<TagsV1> customerResponse = await containerDestination.CreateItemAsync<TagsV1>(item);
                count++;
            }
            
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV1SalesOrder(Database database)
        {
            // *** salesOrder ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("salesOrder", "/id");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/salesOrder.json");
            List<SalesOrderV1> sourceItemList = JsonSerializer.Deserialize<List<SalesOrderV1>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Lets multi-thread all the inserts 
            var tasks = new Task[sourceItemList.Count];
            var taskCount = 0;
            foreach (SalesOrderV1 item in sourceItemList)
            {
                tasks[taskCount] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.id));
                taskCount++;
            }
            await Task.WhenAll(tasks);

            /*
            // Lets insert one record at a time
            int count = 0;
            foreach (SalesOrderV1 item in sourceItemList)
            {
                ItemResponse<SalesOrderV1> customerResponse = await containerDestination.CreateItemAsync<SalesOrderV1>(item);
                count++;
            }
            */
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV1SalesOrderDetail(Database database)
        {
            // *** salesOrderDetail ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("salesOrderDetail", "/id");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/salesOrderDetail.json");
            List<SalesOrderDetailV1> sourceItemList = JsonSerializer.Deserialize<List<SalesOrderDetailV1>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Lets multi-thread all the inserts 
            var tasks = new Task[sourceItemList.Count];
            var taskCount = 0;
            foreach (SalesOrderDetailV1 item in sourceItemList)
            {
                tasks[taskCount] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.id));
                taskCount++;
            }
            await Task.WhenAll(tasks);

            /*
            // Lets insert one record at a time
            int count = 0;
            foreach (SalesOrderDetailV1 item in sourceItemList)
            {
                ItemResponse<SalesOrderDetailV1> customerResponse = await containerDestination.CreateItemAsync<SalesOrderDetailV1>(item);
                count++;
            }
            */
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        #endregion

        public static async Task LoadDatabaseV2Async()
        {
            try
            {
                Database database = null;

                if (serverless)
                {
                    //Create a new database using serverless configuration 
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v2");
                }
                else
                {
                    // Autoscale throughput settings
                    ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(throughPut); //Set autoscale max RU/s

                    //Create a new database with autoscale enabled
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v2", throughputProperties: autoscaleThroughputProperties);
                    Console.WriteLine("Created Database: {0}\n", database.Id);
                }

                #region ParallelTasks-LoadDatabaseV2

                // Perform five tasks in parallel
                Stopwatch stopwatch = Stopwatch.StartNew();
                Task t1 = LoadDatabaseV2Customer(database);
                Task t2 = LoadDatabaseV2Product(database);
                Task t3 = LoadDatabaseV2ProductCategory(database);
                Task t4 = LoadDatabaseV2ProductTag(database);
                Task t5 = LoadDatabaseV2SalesOrder(database);

                await Task.WhenAll(t1, t2, t3, t4, t5);
                stopwatch.Stop();
                Console.WriteLine("All database-v2 containers loaded in {0}\npress any key to exit.\n", stopwatch.Elapsed);

                #endregion
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
                finally
            {
                Console.ReadKey();
            }
        }

        #region LoadDatabaseV2-Containers
        public static async Task LoadDatabaseV2Customer(Database database)
        {
            //*** Customers ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("customer", "/id");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v2/customer.json");
            List<CustomerV2> sourceItemList = JsonSerializer.Deserialize<List<CustomerV2>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Lets multi-thread all the inserts 
            var tasks = new Task[sourceItemList.Count];
            var taskCount = 0;
            foreach (CustomerV2 item in sourceItemList)
            {
                tasks[taskCount] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.id));
                taskCount++;
            }
            await Task.WhenAll(tasks);

            /*
            // Lets insert one record at a time
            int count = 0;
            foreach (CustomerV2 item in sourceItemList)
            {
                ItemResponse<CustomerV2> customerResponse = await containerDestination.CreateItemAsync<CustomerV2>(item);
                count++;
            }
            */
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV2Product(Database database)
        {
            // *** product ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("product", "/categoryId");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v2/product.json");
            List<Product> sourceItemList = JsonSerializer.Deserialize<List<Product>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();
            int count = 0;
            foreach (Product item in sourceItemList)
            {
                ItemResponse<Product> productsResponse = await containerDestination.CreateItemAsync<Product>(item);
                count++;
            }
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV2ProductCategory(Database database)
        {
            // *** productCategory ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("productCategory", "/type");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v2/productCategory.json");
            List<ProductCategory> sourceItemList = JsonSerializer.Deserialize<List<ProductCategory>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();
            int count = 0;
            foreach (ProductCategory item in sourceItemList)
            {
                ItemResponse<ProductCategory> productCategorysResponse = await containerDestination.CreateItemAsync<ProductCategory>(item);
                count++;
            }
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV2ProductTag(Database database)
        {
            // *** producTag ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("productTag", "/type");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v2/productTag.json");
            List<TagV2> sourceItemList = JsonSerializer.Deserialize<List<TagV2>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();
            int count = 0;
            foreach (TagV2 item in sourceItemList)
            {
                ItemResponse<TagV2> productTagResponse = await containerDestination.CreateItemAsync<TagV2>(item);
                count++;
            }
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV2SalesOrder(Database database)
        {
            // *** salesOrder ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("salesOrder", "/customerId");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v2/salesOrder.json");
            List<SalesOrder> sourceItemList = JsonSerializer.Deserialize<List<SalesOrder>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Lets multi-thread all the inserts 
            var tasks = new Task[sourceItemList.Count];
            var taskCount = 0;
            foreach (SalesOrder item in sourceItemList)
            {
                tasks[taskCount] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.customerId));
                taskCount++;
            }
            await Task.WhenAll(tasks);

            /*
            // Lets insert one record at a time
            int count = 0;
            foreach (SalesOrder item in sourceItemList)
            {
                ItemResponse<SalesOrder> salesOrdersResponse = await containerDestination.CreateItemAsync<SalesOrder>(item);
                count++;
            }
            */
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }
        #endregion

        public static async Task LoadDatabaseV3Async()
        {
            try
            {
                Database database = null;

                if (serverless)
                {
                    //Create a new database using serverless configuration 
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v3");
                }
                else
                {
                    // Autoscale throughput settings
                    ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(throughPut); //Set autoscale max RU/s

                    //Create a new database with autoscale enabled
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v3", throughputProperties: autoscaleThroughputProperties);
                    Console.WriteLine("Created Database: {0}\n", database.Id);
                }

                #region ParallelTasks-LoadDatabaseV3

                // Perform five tasks in parallel
                Stopwatch stopwatch = Stopwatch.StartNew();
                Task t1 = LoadDatabaseV3Customer(database);
                Task t2 = LoadDatabaseV3Product(database);
                Task t3 = LoadDatabaseV3ProductCategory(database);
                Task t4 = LoadDatabaseV3ProductTag(database);
                Task t5 = LoadDatabaseV3SalesOrder(database);

                await Task.WhenAll(t1, t2, t3, t4, t5);
                stopwatch.Stop();
                Console.WriteLine("All database-v3 containers loaded in {0}\npress any key to exit.\n", stopwatch.Elapsed);

                #endregion
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.ReadKey();
            }
        }

        #region LoadDatabaseV3-Containers
        public static async Task LoadDatabaseV3Customer(Database database)
        {
            //*** Customers ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("customer", "/id");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v3/customer.json");
            List<CustomerV2> sourceItemList = JsonSerializer.Deserialize<List<CustomerV2>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Lets multi-thread all the inserts 
            var tasks = new Task[sourceItemList.Count];
            var taskCount = 0;
            foreach (CustomerV2 item in sourceItemList)
            {
                tasks[taskCount] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.id));
                taskCount++;
            }
            await Task.WhenAll(tasks);

            /*
            // Lets insert one record at a time
            int count = 0;
            foreach (CustomerV2 item in sourceItemList)
            {
                ItemResponse<CustomerV2> customerResponse = await containerDestination.CreateItemAsync<CustomerV2>(item);
                count++;
            }
            */
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV3Product(Database database)
        {
            // *** product ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("product", "/categoryId");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v3/product.json");
            List<Product> sourceItemList = JsonSerializer.Deserialize<List<Product>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();
            int count = 0;
            foreach (Product item in sourceItemList)
            {
                ItemResponse<Product> productsResponse = await containerDestination.CreateItemAsync<Product>(item);
                count++;
            }
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");

        }

        public static async Task LoadDatabaseV3ProductCategory(Database database)
        {
            // *** productCategory ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("productCategory", "/type");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v3/productCategory.json");
            List<ProductCategory> sourceItemList = JsonSerializer.Deserialize<List<ProductCategory>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();
            int count = 0;
            foreach (ProductCategory item in sourceItemList)
            {
                ItemResponse<ProductCategory> productCategorysResponse = await containerDestination.CreateItemAsync<ProductCategory>(item);
                count++;
            }
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV3ProductTag(Database database)
        {
            // *** producTag ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("productTag", "/type");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v3/productTag.json");
            List<TagV2> sourceItemList = JsonSerializer.Deserialize<List<TagV2>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();
            int count = 0;
            foreach (TagV2 item in sourceItemList)
            {
                ItemResponse<TagV2> productTagResponse = await containerDestination.CreateItemAsync<TagV2>(item);
                count++;
            }
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV3SalesOrder(Database database)
        {
            // *** salesOrder ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("salesOrder", "/customerId");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v3/salesOrder.json");
            List<SalesOrder> sourceItemList = JsonSerializer.Deserialize<List<SalesOrder>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Lets multi-thread all the inserts 
            var tasks = new Task[sourceItemList.Count];
            var taskCount = 0;
            foreach (SalesOrder item in sourceItemList)
            {
                tasks[taskCount] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.customerId));
                taskCount++;
            }
            await Task.WhenAll(tasks);

            /*
            // Lets insert one record at a time
            int count = 0;
            foreach (SalesOrder item in sourceItemList)
            {
                ItemResponse<SalesOrder> salesOrdersResponse = await containerDestination.CreateItemAsync<SalesOrder>(item);
                count++;
            }
            */
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }
        #endregion

        public static async Task LoadDatabaseV4Async()
        {
            try
            {
                Database database = null;

                if (serverless)
                {
                    //Create a new database using serverless configuration 
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v4");
                }
                else
                {
                    // Autoscale throughput settings
                    ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(throughPut); //Set autoscale max RU/s

                    //Create a new database with autoscale enabled
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v4", throughputProperties: autoscaleThroughputProperties);
                    Console.WriteLine("Created Database: {0}\n", database.Id);
                }

                #region ParallelTasks-LoadDatabaseV4

                // Perform four tasks in parallel
                Stopwatch stopwatch = Stopwatch.StartNew();
                Task t1 = LoadDatabaseV4Customer(database);
                Task t2 = LoadDatabaseV4Product(database);
                Task t3 = LoadDatabaseV4ProductMeta(database);
                Task t4 = LoadDatabaseV4SalesByCategory(database);

                await Task.WhenAll(t1,t2,t3,t4);
                stopwatch.Stop();
                Console.WriteLine("All database-v4 containers loaded in {0}\npress any key to exit.\n", stopwatch.Elapsed);

                #endregion
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.ReadKey();
            }
        }

        #region LoadDatabaseV4-Containers
        public static async Task LoadDatabaseV4Customer(Database database)
        {
            //*** Customers ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("customer", "/customerId");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v4/customer.json");
            var sourceItemList = JsonSerializer.Deserialize<List<dynamic>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<CustomerV4> customersList = new List<CustomerV4>();
            List<SalesOrder> salesOdersList = new List<SalesOrder>();

            int count = 0;
            foreach (var item in sourceItemList)
            {
                CustomerV4 filterType = JsonSerializer.Deserialize<CustomerV4>(item.ToString());
                if (filterType.type == "customer")
                {
                    customersList.Add(filterType);
                    //ItemResponse<CustomerV4> customerResponse = await containerDestination.CreateItemAsync<CustomerV4>(filterType);

                }
                else if (filterType.type == "salesOrder")
                {
                    SalesOrder newIem = JsonSerializer.Deserialize<SalesOrder>(item.ToString());
                    salesOdersList.Add(newIem);
                    //ItemResponse<SalesOrder> salesOrdersResponse = await containerDestination.CreateItemAsync<SalesOrder>(newIem);
                }
                count++;
            }

            // Lets multi-thread all the inserts 
            var tasksCustomer = new Task[customersList.Count];
            var taskCountCustomer = 0;
            foreach (CustomerV4 item in customersList)
            {
                tasksCustomer[taskCountCustomer] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.customerId));
                taskCountCustomer++;
            }
            await Task.WhenAll(tasksCustomer);

            // Lets multi-thread all the inserts 
            var tasksSalesOrtder = new Task[salesOdersList.Count];
            var taskCountSalesOrder = 0;
            foreach (SalesOrder item in salesOdersList)
            {
                tasksSalesOrtder[taskCountSalesOrder] = containerDestination
                    .CreateItemAsync(item, new PartitionKey(item.customerId));
                taskCountSalesOrder++;
            }
            await Task.WhenAll(tasksSalesOrtder);

            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV4Product(Database database)
        {
            // *** product ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("product", "/categoryId");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v4/product.json");
            List<Product> sourceItemList = JsonSerializer.Deserialize<List<Product>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();
            int count = 0;
            foreach (Product item in sourceItemList)
            {
                ItemResponse<Product> productsResponse = await containerDestination.CreateItemAsync<Product>(item);
                count++;
            }
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV4ProductMeta(Database database)
        {
            // *** productMeta ***
            // Create a new container
            Container containerDestination = await database.CreateContainerIfNotExistsAsync("productMeta", "/type");
            Console.WriteLine("Created Container: {0}\n", containerDestination.Id);
            // Deserialized data file
            string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v4/productMeta.json");
            List<ProductMeta> sourceItemList = JsonSerializer.Deserialize<List<ProductMeta>>(jsonString);
            int ItemsToInsert = sourceItemList.Count;
            Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);
            // Insert into the container
            Stopwatch stopwatch = Stopwatch.StartNew();
            int count = 0;
            foreach (ProductMeta item in sourceItemList)
            {
                ItemResponse<ProductMeta> productMetasResponse = await containerDestination.CreateItemAsync<ProductMeta>(item);
                count++;
            }
            stopwatch.Stop();
            Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
        }

        public static async Task LoadDatabaseV4SalesByCategory(Database database)
        {
            // *** salesByCategory ***
            // Create a new container
            Container containerSalesByCategory = await database.CreateContainerIfNotExistsAsync("salesByCategory", "/categoryId");
            Console.WriteLine("Created Container: {0}\n", containerSalesByCategory.Id);

            // Data gets loaded into salesByCategory container by a change feed process

        }

        #endregion

        public static async Task BulkLoadDatabaseV1Async()
        {
            try
            {
                Database database = null;

                if (serverless)
                {
                    //Create a new database using serverless configuration 
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v1");
                    Console.WriteLine("Created Database: {0}\n", database.Id);
                }
                else
                {
                    // Autoscale throughput settings
                    ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(throughPut); //Set autoscale max RU/s

                    //Create a new database with autoscale enabled
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v1", throughputProperties: autoscaleThroughputProperties);
                    Console.WriteLine("Created Database: {0}\n", database.Id);
                }

                #region ParallelTasks-LoadDatabaseBulk

                // Perform nine tasks in parallel
                Task t1 = BulkLoadDatabaseV1Customer(database);
                Task t2 = BulkLoadDatabaseV1CustomerAddress(database);
                Task t3 = BulkLoadDatabaseV1CustomerPassword(database);
                Task t4 = BulkLoadDatabaseV1Product(database);
                Task t5 = BulkLoadDatabaseV1ProductCategory(database);
                Task t6 = BulkLoadDatabaseV1ProductTag(database);
                Task t7 = BulkLoadDatabaseV1ProductTags(database);
                Task t8 = BulkLoadDatabaseV1SalesOrder(database);
                Task t9 = BulkLoadDatabaseV1SalesOrderDetail(database);

                await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7, t8, t9);

                #endregion

            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("database-v1 containers loaded\npress any key to exit.\n");
                Console.ReadKey();
            }
        }

        #region BulkLoadDatabaseV1-Containers

        public static async Task BulkLoadDatabaseV1Customer(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("customer", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("customer", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** Customers ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("customer", "/id");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);


                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/customer.json");
                List<CustomerV1> sourceItemList = JsonSerializer.Deserialize<List<CustomerV1>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (CustomerV1 item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV1CustomerAddress(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("customerAddress", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("customerAddress", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** CustomerAddress ***
                // Create a new  container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("customerAddress", "/id");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);


                // Deserialized customer data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/customerAddress.json");
                List<CustomerAddress> sourceItemList = JsonSerializer.Deserialize<List<CustomerAddress>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (CustomerAddress item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV1CustomerPassword(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("customerPassword", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("customerPassword", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** CustomerPassword ***
                // Create a new  container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("customerPassword", "/id");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);


                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/customerPassword.json");
                List<PasswordV1> sourceItemList = JsonSerializer.Deserialize<List<PasswordV1>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (PasswordV1 item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV1Product(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("product", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("product", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** Product ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("product", "/id");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);

                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/product.json");
                List<ProductV1> sourceItemList = JsonSerializer.Deserialize<List<ProductV1>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (ProductV1 item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV1ProductCategory(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("productCategory", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("productCategory", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** productCategory ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("productCategory", "/id");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);

                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/productCategory.json");
                List<ProductCategoryV1> sourceItemList = JsonSerializer.Deserialize<List<ProductCategoryV1>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (ProductCategoryV1 item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV1ProductTag(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("productTag", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("productTag", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** productTag ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("productTag", "/id");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);

                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/productTag.json");
                List<Tag> sourceItemList = JsonSerializer.Deserialize<List<Tag>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (Tag item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV1ProductTags(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("productTags", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("productTags", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** Customers ***
                // Create a new customer container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("productTags", "/id");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);


                // Deserialized customer data file
                string jsonStringCustomer = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/productTags.json");
                List<TagsV1> sourceItemList = JsonSerializer.Deserialize<List<TagsV1>>(jsonStringCustomer);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (TagsV1 item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV1SalesOrder(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("salesOrder", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("salesOrder", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** SalesOrders ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("salesOrder", "/id");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);

                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/salesOrder.json");
                List<SalesOrderV1> sourceItemList = JsonSerializer.Deserialize<List<SalesOrderV1>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (SalesOrderV1 item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV1SalesOrderDetail(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("salesOrderDetail", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("salesOrderDetail", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** SalesOrderDetail ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("salesOrderDetail", "/id");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);


                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v1/salesOrderDetail.json");
                List<SalesOrderDetailV1> sourceItemList = JsonSerializer.Deserialize<List<SalesOrderDetailV1>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (SalesOrderDetailV1 item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        #endregion

        public static async Task BulkLoadDatabaseV2Async()
        {
            try
            {
                Database database = null;

                if (serverless)
                {
                    //Create a new database using serverless configuration 
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v2");
                    Console.WriteLine("Created Database: {0}\n", database.Id);
                }
                else
                {
                    // Autoscale throughput settings
                    ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(throughPut); //Set autoscale max RU/s

                    //Create a new database with autoscale enabled
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v2", throughputProperties: autoscaleThroughputProperties);
                    Console.WriteLine("Created Database: {0}\n", database.Id);
                }

                #region ParallelTasks-LoadDatabaseBulk

                // Perform five tasks in parallel
                Task t1 = BulkLoadDatabaseV2Customer(database);
                Task t2 = BulkLoadDatabaseV2Product(database);
                Task t3 = BulkLoadDatabaseV2ProductCategory(database);
                Task t4 = BulkLoadDatabaseV2ProductTag(database);
                Task t5 = BulkLoadDatabaseV2SalesOrder(database);

                await Task.WhenAll(t1, t2, t3, t4, t5);

                #endregion

            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("database-v2 containers loaded\npress any key to exit.\n");
                Console.ReadKey();
            }
        }

        #region BulkLoadDatabaseV2-Containers

        public static async Task BulkLoadDatabaseV2Customer(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("customer", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("customer", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }

                //*** Customers ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("customer", "/id");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);

                // Deserialized data file
                string jsonStringCustomer = File.ReadAllText(filePath + "cosmosdb-adventureworks-v2/customer.json");
                List<CustomerV2> sourceItemList = JsonSerializer.Deserialize<List<CustomerV2>>(jsonStringCustomer);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (CustomerV2 item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV2Product(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("product", "/categoryId")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("product", "/categoryId")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** Product ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("product", "/categoryId");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);


                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v2/product.json");
                List<Product> sourceItemList = JsonSerializer.Deserialize<List<Product>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (Product item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV2ProductCategory(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("productCategory", "/type")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("productCategory", "/type")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** poductCategory ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("productCategory", "/type");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);


                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v2/productCategory.json");
                List<ProductCategory> sourceItemList = JsonSerializer.Deserialize<List<ProductCategory>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (ProductCategory item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                    Thread.Sleep(2000);
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV2ProductTag(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("productTag", "/type")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("productTag", "/type")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** productTag ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("productTag", "/type");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);

                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v2/productTag.json");
                List<TagV2> sourceItemList = JsonSerializer.Deserialize<List<TagV2>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (TagV2 item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                    Thread.Sleep(2000);
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV2SalesOrder(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("salesOrder", "/customerId")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("salesOrder", "/customerId")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }

                //*** SalesOrders ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("salesOrder", "/customerId");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);

                // Deserialized customer data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v2/salesOrder.json");
                List<SalesOrder> sourceItemList = JsonSerializer.Deserialize<List<SalesOrder>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (SalesOrder item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();
                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
        }

        #endregion

        public static async Task BulkLoadDatabaseV3Async()
        {
            try
            {
                Database database = null;

                if (serverless)
                {
                    //Create a new database using serverless configuration 
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v3");
                    Console.WriteLine("Created Database: {0}\n", database.Id);
                }
                else
                {
                    // Autoscale throughput settings
                    ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(throughPut); //Set autoscale max RU/s

                    //Create a new database with autoscale enabled
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v3", throughputProperties: autoscaleThroughputProperties);
                    Console.WriteLine("Created Database: {0}\n", database.Id);
                }

                #region ParallelTasks-LoadDatabaseBulk

                // Perform five tasks in parallel
                Task t1 = BulkLoadDatabaseV3Customer(database);
                Task t2 = BulkLoadDatabaseV3Product(database);
                Task t3 = BulkLoadDatabaseV3ProductCategory(database);
                Task t4 = BulkLoadDatabaseV3ProductTag(database);
                Task t5 = BulkLoadDatabaseV3SalesOrder(database);

                await Task.WhenAll(t1, t2, t3, t4, t5);

                #endregion

            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("database-v2 containers loaded\npress any key to exit.\n");
                Console.ReadKey();
            }
        }

        #region BulkLoadDatabaseV3-Containers

        public static async Task BulkLoadDatabaseV3Customer(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("customer", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("customer", "/id")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** Customers ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("customer", "/id");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);

                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v3/customer.json");
                List<CustomerV2> sourceItemList = JsonSerializer.Deserialize<List<CustomerV2>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (CustomerV2 item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();
                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV3Product(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("product", "/categoryId")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("product", "/categoryId")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** product ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("product", "/categoryId");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);


                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v3/product.json");
                List<Product> sourceItemList = JsonSerializer.Deserialize<List<Product>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (Product item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV3ProductCategory(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("productCategory", "/type")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("productCategory", "/type")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** productCategory ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("productCategory", "/type");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);


                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v3/productCategory.json");
                List<ProductCategory> sourceItemList = JsonSerializer.Deserialize<List<ProductCategory>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (ProductCategory item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                    Thread.Sleep(2000);
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV3ProductTag(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("productTag", "/type")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("productTag", "/type")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** productTag ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("productTag", "/type");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);


                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v3/productTag.json");
                List<TagV2> sourceItemList = JsonSerializer.Deserialize<List<TagV2>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (TagV2 item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                    Thread.Sleep(2000);
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV3SalesOrder(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("salesOrder", "/customerId")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("salesOrder", "/customerId")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** Customers ***
                // Create a new  container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("salesOrder", "/customerId");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);


                // Deserialized customer data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v3/salesOrder.json");
                List<SalesOrder> sourceItemList = JsonSerializer.Deserialize<List<SalesOrder>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (SalesOrder item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        #endregion

        public static async Task BulkLoadDatabaseV4Async()
        {
            try
            {
                Database database = null;

                if (serverless)
                {
                    //Create a new database using serverless configuration 
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v4");
                    Console.WriteLine("Created Database: {0}\n", database.Id);
                }
                else
                {
                    // Autoscale throughput settings
                    ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(throughPut); //Set autoscale max RU/s

                    //Create a new database with autoscale enabled
                    database = await client.CreateDatabaseIfNotExistsAsync("database-v4", throughputProperties: autoscaleThroughputProperties);
                    Console.WriteLine("Created Database: {0}\n", database.Id);
                }

                #region ParallelTasks-LoadDatabaseBulk

                // Perform five tasks in parallel
                Task t1 = BulkLoadDatabaseV4Customer(database);
                Task t2 = BulkLoadDatabaseV4Product(database);
                Task t3 = BulkLoadDatabaseV4ProductMeta(database);
                Task t4 = BulkLoadDatabaseV4SalesOrder(database);
                Task t5 = LoadDatabaseV4SalesByCategory(database);

                await Task.WhenAll(t1, t2, t3, t4, t5);

                #endregion

            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("database-v4 containers loaded\npress any key to exit.\n");
                Console.ReadKey();
            }
        }

        #region BulkLoadDatabaseV4-Containers

        public static async Task BulkLoadDatabaseV4Customer(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("customer", "/customerId")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("customer", "/customerId")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** Customers ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("customer", "/customerId");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);

                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v4/customer.json");
                var sourceItemList = JsonSerializer.Deserialize<List<dynamic>>(jsonString);
                int ItemsToInsertCount = sourceItemList.Count;
                int ItemsToInsertCustomerCount = 0;
                int ItemsToInsertSalesOrderCount = 0;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsertCount);

                // Prepare items for insertion
                List<CustomerV4> sourceItemListCustomer = new List<CustomerV4>();
                List<SalesOrder> sourceItemListSales = new List<SalesOrder>();

                //Build up the filtered items list from source list
                foreach (var item in sourceItemList)
                {
                    CustomerV4 filterType = JsonSerializer.Deserialize<CustomerV4>(item.ToString());
                    if (filterType.type == "customer")
                    {
                        sourceItemListCustomer.Add(filterType);
                        ItemsToInsertCustomerCount++;
                    }
                    else if (filterType.type == "salesOrder")
                    {
                        SalesOrder newItem = JsonSerializer.Deserialize<SalesOrder>(item.ToString());
                        sourceItemListSales.Add(newItem);
                        ItemsToInsertSalesOrderCount++;
                    }
                }

                //Load customer list into dictionary
                Dictionary<PartitionKey, Stream> itemsToInsertCustomer = new Dictionary<PartitionKey, Stream>(sourceItemListCustomer.Count);
                foreach (CustomerV4 itemC in sourceItemListCustomer)
                {
                    MemoryStream streamCustomer = new MemoryStream();
                    await JsonSerializer.SerializeAsync(streamCustomer, itemC);
                    itemsToInsertCustomer.Add(new PartitionKey(itemC.id), streamCustomer);
                }

                // Create the list of Tasks
                Stopwatch stopwatchC = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasksCustomer = new List<Task>(ItemsToInsertCustomerCount);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsertCustomer)
                {
                    tasksCustomer.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }
                // Wait until all are done
                await Task.WhenAll(tasksCustomer);
                // </ConcurrentTasks>
                stopwatchC.Stop();
                Console.WriteLine($"Finished writing [{ItemsToInsertCustomerCount}] items into [{containerDestination.Id}] container in {stopwatchC.Elapsed}.\n");

            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV4SalesOrder(Database database)
        {
            try
            {
                Thread.Sleep(3000);
                //*** saleOrders in Customers ***
                Container containerDestination = database.GetContainer("customer");

                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v4/customer.json");
                var sourceItemList = JsonSerializer.Deserialize<List<dynamic>>(jsonString);
                int ItemsToInsertCount = sourceItemList.Count;
                int ItemsToInsertCustomerCount = 0;
                int ItemsToInsertSalesOrderCount = 0;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsertCount);

                // Prepare items for insertion
                List<CustomerV4> sourceItemListCustomer = new List<CustomerV4>();
                List<SalesOrder> sourceItemListSales = new List<SalesOrder>();

                //Build up the filtered items list from source list
                foreach (var item in sourceItemList)
                {
                    CustomerV4 filterType = JsonSerializer.Deserialize<CustomerV4>(item.ToString());
                    if (filterType.type == "customer")
                    {
                        sourceItemListCustomer.Add(filterType);
                        ItemsToInsertCustomerCount++;
                    }
                    else if (filterType.type == "salesOrder")
                    {
                        SalesOrder newItem = JsonSerializer.Deserialize<SalesOrder>(item.ToString());
                        sourceItemListSales.Add(newItem);
                        ItemsToInsertSalesOrderCount++;
                    }
                }

                //Load salesOrder list into dictionary
                Dictionary<PartitionKey, Stream> itemsToInsertCustomer = new Dictionary<PartitionKey, Stream>(sourceItemListSales.Count);
                foreach (SalesOrder itemC in sourceItemListSales)
                {
                    MemoryStream streamCustomer = new MemoryStream();
                    await JsonSerializer.SerializeAsync(streamCustomer, itemC);
                    itemsToInsertCustomer.Add(new PartitionKey(itemC.id), streamCustomer);
                }

                // Create the list of Tasks
                Stopwatch stopwatchC = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasksCustomer = new List<Task>(ItemsToInsertCustomerCount);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsertCustomer)
                {
                    tasksCustomer.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }
                // Wait until all are done
                await Task.WhenAll(tasksCustomer);
                // </ConcurrentTasks>
                stopwatchC.Stop();
                Console.WriteLine($"Finished writing [{ItemsToInsertCustomerCount}] items into [{containerDestination.Id}] container in {stopwatchC.Elapsed}.\n");

            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV4Product(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("product", "/categoryId")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("product", "/categoryId")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** product ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("product", "/categoryId");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);


                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v4/product.json");
                List<Product> sourceItemList = JsonSerializer.Deserialize<List<Product>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (Product item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();
                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        public static async Task BulkLoadDatabaseV4ProductMeta(Database database)
        {
            try
            {
                if (serverless)
                {
                    await database.DefineContainer("productMeta", "/type")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync();
                }
                else
                {
                    await database.DefineContainer("productMeta", "/type")
                            .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                                .Attach()
                            .CreateAsync(throughPut);
                }
                //*** productCategory ***
                // Create a new container
                Container containerDestination = await database.CreateContainerIfNotExistsAsync("productMeta", "/type");
                Console.WriteLine("Created Container: {0}\n", containerDestination.Id);

                // Deserialized data file
                string jsonString = File.ReadAllText(filePath + "cosmosdb-adventureworks-v4/productMeta.json");
                List<ProductMeta> sourceItemList = JsonSerializer.Deserialize<List<ProductMeta>>(jsonString);
                int ItemsToInsert = sourceItemList.Count;
                Console.WriteLine("Deserialized [{0}] data [{1}] items\n", containerDestination.Id, ItemsToInsert);

                // Prepare items for insertion
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (ProductMeta item in sourceItemList)
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.id), stream);
                }

                // Create the list of Tasks
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(containerDestination.Id);
                List<Task> tasks = new List<Task>(ItemsToInsert);
                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using (ResponseMessage response = task.Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                                    Thread.Sleep(2000);
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished writing [{ItemsToInsert}] items into [{containerDestination.Id}] container in {stopwatch.Elapsed}.\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

        #endregion

        #region Helper functions

        protected async Task<int> WriteItems(Container container, List<CustomerV1> docs)
        {
            var tasks = new Task[docs.Count];
            var taskCtr = 0;
            var errorCtr = 0;
            foreach (var item in docs)
            {
                tasks[taskCtr] = container
                    .CreateItemAsync(item, new PartitionKey(item.id));
                    //.ContinueWith(task => errorCtr = this.CatchInsertDocumentError(task, errorCtr));

                taskCtr++;
            }

            await Task.WhenAll(tasks);
            return errorCtr;
        }


        #endregion

    }
}
