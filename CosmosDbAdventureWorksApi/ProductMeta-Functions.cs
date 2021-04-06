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
    public static class ProductMetaFunction
    {
        #region ListAllProductCategory
        [FunctionName("ListAllProductCategories")]
        public static IActionResult RunListAllProductCategories(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "database-v4",
                collectionName: "productMeta",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c WHERE c.type = 'category'")]
                IEnumerable<ProductMeta> outputItems, ILogger log)
        {
            try
            {
                if (outputItems is null)
                {
                    return new NotFoundResult();
                }

                log.LogInformation("ListAllProductCategories function processed a request.");
                return new OkObjectResult(outputItems);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region ListAllProductTags
        [FunctionName("ListAllProductTags")]
        public static IActionResult RunListAllProductTags(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "database-v4",
                collectionName: "productMeta",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c WHERE c.type = 'tag'")]
                IEnumerable<ProductMeta> outputItems, ILogger log)
        {
            try
            {
                if (outputItems is null)
                {
                    return new NotFoundResult();
                }

                log.LogInformation("ListAllProductTags function processed a request.");
                return new OkObjectResult(outputItems);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region GetProductCategoryById 
        [FunctionName("GetProductCategoryById")]
        public static IActionResult RunGetProductCategoryById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "GetProductCategoryById/{id}")]HttpRequest req,
        [CosmosDB(
                databaseName: "database-v4",
                collectionName: "productMeta",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "{id}",
                PartitionKey = "category")] ProductMeta outputDocument,
        ILogger log)
        {
            try
            {
                if (outputDocument is null)
                {
                    return new NotFoundResult();
                }

                log.LogInformation("GetProductCategoryById function processed a request.");
                return new OkObjectResult(outputDocument);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region GetProductTagById 
        [FunctionName("GetProductTagById")]
        public static IActionResult RunGetProductTagById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "GetProductTagById/{id}")]HttpRequest req,
        [CosmosDB(
                databaseName: "database-v4",
                collectionName: "productMeta",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "{id}",
                PartitionKey = "tag")] ProductMeta outputDocument,
        ILogger log)
        {
            try
            {
                if (outputDocument is null)
                {
                    return new NotFoundResult();
                }

                log.LogInformation("GetProductTagById function processed a request.");
                return new OkObjectResult(outputDocument);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region UpsertProductCategory
        [FunctionName("UpsertProductCategory")]
        public static async Task<IActionResult> RunUpsertProductCategory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post",
                Route = "UpsertProductCategory/upsert")] HttpRequest req,
        [CosmosDB(
                databaseName: "database-v4",
                collectionName: "productMeta",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<ProductMeta> collector,
        ILogger log)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<ProductMeta>(
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

        #region UpsertProductTag
        [FunctionName("UpsertProductTag")]
        public static async Task<IActionResult> RunUpsertProductTag(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post",
                Route = "UpsertProductTag/upsert")] HttpRequest req,
        [CosmosDB(
                databaseName: "database-v4",
                collectionName: "productMeta",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<ProductMeta> collector,
        ILogger log)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<ProductMeta>(
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

        #region DeleteProductCategory
        [FunctionName("DeleteProductCategory")]
        public static async Task<IActionResult> RunDeleteProductCategory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete",
                Route = "DeleteProductCategory/delete/{id}")] HttpRequest req,
        [CosmosDB(
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
        string id,
        ILogger log)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<ProductMeta>(
                        await new StreamReader(req.Body).ReadToEndAsync());

                Uri collectionUri = UriFactory.CreateDocumentUri("database-v4", "productMeta", id);
                await documentClient.DeleteDocumentAsync(collectionUri, new RequestOptions { PartitionKey = new PartitionKey("category") });
                return new OkObjectResult("Data Deleted");

            }
            catch (DocumentClientException ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        #endregion

        #region DeleteProductTag
        [FunctionName("DeleteProductTag")]
        public static async Task<IActionResult> RunDeleteProductTag(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete",
                Route = "DeleteProductTag/delete/{id}")] HttpRequest req,
        [CosmosDB(
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
        string id,
        ILogger log)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<ProductMeta>(
                        await new StreamReader(req.Body).ReadToEndAsync());

                Uri collectionUri = UriFactory.CreateDocumentUri("database-v4", "productMeta", id);
                await documentClient.DeleteDocumentAsync(collectionUri, new RequestOptions { PartitionKey = new PartitionKey("tag") });
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

    public class ProductMeta
    {

        public string id { get; set; }
        [Required, MinLength(3), MaxLength(75)]
        public string name { get; set; }
        public string type { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public string _attachments { get; set; }
        public int _ts { get; set; }

    }

}

