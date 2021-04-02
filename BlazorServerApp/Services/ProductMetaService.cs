using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorServerApp.Models;
using Microsoft.AspNetCore.Components;


namespace BlazorServerApp.Services
{
    public class ProductMetaService : IProductMetaService
    {
        private readonly HttpClient httpClient;

        public ProductMetaService(HttpClient httpClient) 
        {
            this.httpClient = httpClient;
        }

        public async Task<ProductMeta> GetProductCategory(string id)
        {
            try
            {
                return await httpClient.GetJsonAsync<ProductMeta>($"api/GetProductCategoryById/{id}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<IEnumerable<ProductMeta>> ListAllProductCategories()
        {
            return await httpClient.GetJsonAsync<ProductMeta[]>("api/ListAllProductCategories");
         }
    }
}
