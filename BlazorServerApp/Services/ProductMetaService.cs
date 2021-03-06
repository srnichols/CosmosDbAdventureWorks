﻿using System;
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

        public async Task<ProductMeta> CreateProductCategory(ProductMeta createCategory)
        {
            try
            {
                return await httpClient.PostJsonAsync<ProductMeta>($"api/UpsertProductCategory/upsert", createCategory);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<ProductMeta> CreateProductTag(ProductMeta createTag)
        {
            try
            {
                return await httpClient.PostJsonAsync<ProductMeta>($"api/UpsertProductTag/upsert", createTag);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<HttpResponseMessage> DeleteProductCategory(string id)
        {
            return await httpClient.DeleteAsync($"api/DeleteProductCategory/delete/{id}");
        }

        public async Task<HttpResponseMessage> DeleteProductTag(string id)
        {
            return await httpClient.DeleteAsync($"api/DeleteProductTag/delete/{id}");
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

        public async Task<ProductMeta> GetProductTag(string id)
        {
            try
            {
                return await httpClient.GetJsonAsync<ProductMeta>($"api/GetProductTagById/{id}");
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

        public async Task<IEnumerable<ProductMeta>> ListAllProductTags()
        {
            return await httpClient.GetJsonAsync<ProductMeta[]>("api/ListAllProductTags");
        }

        public async Task<ProductMeta> UpdateProductCategory(ProductMeta updateCategory)
        {
            try
            {
                return await httpClient.PostJsonAsync<ProductMeta>($"api/UpsertProductCategory/upsert", updateCategory);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<ProductMeta> UpdateProductTag(ProductMeta updateTag)
        {
            try
            {
                return await httpClient.PostJsonAsync<ProductMeta>($"api/UpsertProductTag/upsert", updateTag);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }
    }
}
