using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorServerApp.Models;
using Microsoft.AspNetCore.Components;


namespace BlazorServerApp.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient httpClient;

        public ProductService(HttpClient httpClient) 
        {
            this.httpClient = httpClient;
        }

        public async Task<Product> CreateProduct(Product createProduct)
        {
            try
            {
                return await httpClient.PostJsonAsync<Product>($"api/UpsertProduct/upsert", createProduct);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<HttpResponseMessage> DeleteProduct(string id, string pk)
        {
            return await httpClient.DeleteAsync($"api/DeleteProduct/delete/{id}/{pk}");
        }

        public async Task<Product> GetProduct(string id, string pk)
        {
            try
            {
                return await httpClient.GetJsonAsync<Product>($"api/GetProductById/{id}/{pk}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<IEnumerable<Product>> ListAllProducts()
        {
            return await httpClient.GetJsonAsync<Product[]>("api/ListAllProducts");
         }

        public async Task<Product> UpdateProduct(Product updateProduct)
        {
            try
            {
                return await httpClient.PostJsonAsync<Product>($"api/UpsertProduct/upsert", updateProduct);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

    }
}
