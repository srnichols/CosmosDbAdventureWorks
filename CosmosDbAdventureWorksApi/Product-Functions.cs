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
    public static class ProductFunction
    {
        #region ListAllProducts
        [FunctionName("ListAllProducts")]
        public static IActionResult RunListAllProducts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "database-v4",
                collectionName: "product",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c")]
                IEnumerable<Product> outputItems, ILogger log)
        {
            try
            {
                if (outputItems is null)
                {
                    return new NotFoundResult();
                }

                log.LogInformation("ListAllProducts function processed a request.");
                return new OkObjectResult(outputItems);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region GetProductById 
        [FunctionName("GetProductById")]
        public static IActionResult RunGetProductById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "GetProductById/{id}/{pk}")]HttpRequest req,
        [CosmosDB(
                databaseName: "database-v4",
                collectionName: "product",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "{id}",
                PartitionKey = "{pk}")] Product outputDocument,
        ILogger log)
        {
            try
            {
                if (outputDocument is null)
                {
                    return new NotFoundResult();
                }

                log.LogInformation("GetProductById function processed a request.");
                return new OkObjectResult(outputDocument);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region UpsertProduct
        [FunctionName("UpsertProduct")]
        public static async Task<IActionResult> RunUpsertProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post",
                Route = "UpsertProduct/upsert")] HttpRequest req,
        [CosmosDB(
                databaseName: "database-v4",
                collectionName: "product",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<Product> collector,
        ILogger log)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Product>(
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

        #region DeleteProduct
        [FunctionName("DeleteProduct")]
        public static async Task<IActionResult> RunDeleteProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete",
                Route = "DeleteProduct/delete/{id}/{pk}")] HttpRequest req,
        [CosmosDB(
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
        string id,
        string pk,
        ILogger log)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Product>(
                        await new StreamReader(req.Body).ReadToEndAsync());

                Uri collectionUri = UriFactory.CreateDocumentUri("database-v4", "product", id);
                await documentClient.DeleteDocumentAsync(collectionUri, new RequestOptions { PartitionKey = new PartitionKey(pk) });
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

    public class Product
    {
        [Required]
        public string id { get; set; }
        [Required]
        public string categoryId { get; set; }
        public string categoryName { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(25)]
        public string sku { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(75)]
        public string name { get; set; }
        [Required]
        [MinLength(5)]
        [MaxLength(250)]
        public string description { get; set; }
        [Required]
        public double price { get; set; }
        public List<Tag> tags { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public string _attachments { get; set; }
        public int _ts { get; set; }
    }

    public class Tag
    {
        public string id { get; set; }
        public string name { get; set; }
    }

}

