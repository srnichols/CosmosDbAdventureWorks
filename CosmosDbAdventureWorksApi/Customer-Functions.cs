using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace CosmosDbAdventureWorksApi

{
    public static class CustomerFunction
    {
        #region ListAllCustomers
        [FunctionName("ListAllCustomers")]
        public static IActionResult RunListAllCustomers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "database-v4",
                collectionName: "customer",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c WHERE c.type = 'customer' ORDER By c.lastName")]
                IEnumerable<Customer> outputItems, ILogger log)
        {
            try
            {
                if (outputItems is null)
                {
                    return new NotFoundResult();
                }

                log.LogInformation("ListAllCustomers function request processed.");
                return new OkObjectResult(outputItems);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region ListCustomerHeaderByLastNameSearch
        [FunctionName("ListCustomerHeaderByLastNameSearch")]
        public static IActionResult RunListCustomerHeaderByLastNameSearch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "ListCustomerHeaderByLastNameSearch/{searchTerm}")] HttpRequest req,
            [CosmosDB(
                databaseName: "database-v4",
                collectionName: "customer",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT c. customerId, c.title, c.firstName, c.lastName, c.emailAddress, c.addresses[0].city, c.addresses[0].state FROM c WHERE c.type = 'customer' and c.lastName Like {searchTerm} ORDER BY c.lastName, c.firstName")]
                IEnumerable<CustomerHeader> outputItems, ILogger log)
        {
            try
            {
                if (outputItems is null)
                {
                    return new NotFoundResult();
                }

                log.LogInformation("ListCustomerHeaderByLastNameSearch function processed a request.");
                return new OkObjectResult(outputItems);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region ListAllSalesOrders
        [FunctionName("ListAllSalesOrders")]
        public static IActionResult RunListAllSalesOrders(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "database-v4",
                collectionName: "customer",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c WHERE c.type = 'salesOrder' ORDER BY c.orderDate DESC")]
                IEnumerable<SalesOrder> outputItems, ILogger log)
        {
            try
            {
                if (outputItems is null)
                {
                    return new NotFoundResult();
                }

                log.LogInformation("ListAllSalesOrders function processed a request.");
                return new OkObjectResult(outputItems);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region ListAllSalesOrdersByCustomerID
        [FunctionName("ListAllSalesOrdersByCustomerID")]
        public static IActionResult RunListAllSalesOrdersByCustomerID(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "ListAllSalesOrdersByCustomerID/{id}")] HttpRequest req,
            [CosmosDB(
                databaseName: "database-v4",
                collectionName: "customer",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c WHERE c.type = 'salesOrder' and c.customerId = {id} ORDER BY c.orderDate DESC")]
                IEnumerable<SalesOrder> outputItems, ILogger log)
        {
            try
            {
                if (outputItems is null)
                {
                    return new NotFoundResult();
                }

                log.LogInformation("ListAllSalesOrdersByCustomerId function processed a request.");
                return new OkObjectResult(outputItems);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region GetCustomerById 
        [FunctionName("GetCustomerById")]
        public static IActionResult RunGetCustomerById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "GetCustomerById/{id}")]HttpRequest req,
        [CosmosDB(
                databaseName: "database-v4",
                collectionName: "customer",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "{id}",
                PartitionKey = "{id}")] Customer outputDocument,
        ILogger log)
        {
            try
            {
                if (outputDocument is null)
                {
                    return new NotFoundResult();
                }

                log.LogInformation("GetCustomerById function processed a request.");
                return new OkObjectResult(outputDocument);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region GetSalesOrderById 
        [FunctionName("GetSalesOrderById")]
        public static IActionResult RunGetSalesOrderById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "GetSalesOrderById/{id}")]HttpRequest req,
        [CosmosDB(
                databaseName: "database-v4",
                collectionName: "customer",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "{id}",
                PartitionKey = "{id}")] SalesOrder outputDocument,
        ILogger log)
        {
            try
            {
                if (outputDocument is null)
                {
                    return new NotFoundResult();
                }

                log.LogInformation("GetSalesOrderById function processed a request.");
                return new OkObjectResult(outputDocument);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region UpsertCustomer
        [FunctionName("UpsertCustomer")]
        public static async Task<IActionResult> RunUpsertCustomer(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post",
                Route = "UpsertCustomer/upsert")] HttpRequest req,
        [CosmosDB(
                databaseName: "database-v4",
                collectionName: "customer",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<Customer> collector,
        ILogger log)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Customer>(
                    await req.ReadAsStringAsync().ConfigureAwait(false));

                await collector.AddAsync(data).ConfigureAwait(false);

                return new OkObjectResult(data);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region UpsertSalesOrder
        [FunctionName("UpsertSalesOrder")]
        public static async Task<IActionResult> RunUpsertSalesOrder(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post",
                Route = "UpsertSalesOrder/upsert")] HttpRequest req,
        [CosmosDB(
                databaseName: "database-v4",
                collectionName: "customer",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<SalesOrder> collector,
        ILogger log)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<SalesOrder>(
                    await req.ReadAsStringAsync().ConfigureAwait(false));

                await collector.AddAsync(data).ConfigureAwait(false);

                return new OkObjectResult(data);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region DeleteCustomer
        [FunctionName("DeleteCustomer")]
        public static async Task<IActionResult> RunDeleteCustomer(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete",
                Route = "DeleteCustomer/delete/{id}")] HttpRequest req,
        [CosmosDB(
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
        string id,
        ILogger log)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Customer>(
                        await new StreamReader(req.Body).ReadToEndAsync());

                Uri collectionUri = UriFactory.CreateDocumentUri("database-v4", "customer", id);
                await documentClient.DeleteDocumentAsync(collectionUri, new RequestOptions { PartitionKey = new PartitionKey(id) });
                return new OkObjectResult("Data Deleted");

            }
            catch (DocumentClientException ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region DeleteSalesOrder
        [FunctionName("DeleteSalesOrder")]
        public static async Task<IActionResult> RunDeleteSalesOrder(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete",
                Route = "DeleteSalesOrder/delete/{id}")] HttpRequest req,
        [CosmosDB(
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
        string id,
        ILogger log)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<SalesOrder>(
                        await new StreamReader(req.Body).ReadToEndAsync());

                Uri collectionUri = UriFactory.CreateDocumentUri("database-v4", "customer", id);
                await documentClient.DeleteDocumentAsync(collectionUri, new RequestOptions { PartitionKey = new PartitionKey(id) });
                return new OkObjectResult("Data Deleted");

            }
            catch (DocumentClientException ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion
    }

    public class Customer
    {
        public string id { get; set; }
        public string type { get; set; }
        public string customerId { get; set; }
        public string title { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string emailAddress { get; set; }
        public string phoneNumber { get; set; }
        public string creationDate { get; set; }
        public List<CustomerAddress> addresses { get; set; }
        public Password password { get; set; }
        public int salesOrderCount { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public string _attachments { get; set; }
        public int _ts { get; set; }

    }

    public class CustomerHeader
    {
        public string customerId { get; set; }
        public string title { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string emailAddress { get; set; }
        public string city { get; set; }
        public string state { get; set; }

    }

    public class CustomerAddress
    {
        public string id { get; set; }
        public string customerId { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string zipCode { get; set; }
        public Location location { get; set; }
    }

    public class Location
    {
        public string type { get; set; }
        public List<float> coordinates { get; set; }
    }

    public class Password
    {
        public string hash { get; set; }
        public string salt { get; set; }
    }

    public class SalesOrder
    {
        public string id { get; set; }
        public string type { get; set; }
        public string customerId { get; set; }
        public string orderDate { get; set; }
        public string shipDate { get; set; }
        public List<SalesOrderDetails> details { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public string _attachments { get; set; }
        public int _ts { get; set; }
    }

    public class SalesOrderDetails
    {
        public string id { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public double price { get; set; }
        public int quantity { get; set; }
    }
}

