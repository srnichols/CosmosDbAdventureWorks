using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorServerApp.Models;

namespace BlazorServerApp.Services
{
    public interface IProductMetaService
    {
        Task<IEnumerable<ProductMeta>> ListAllProductCategories();
        Task<ProductMeta> GetProductCategory(string id);
        Task<ProductMeta> UpdateProductCategory(ProductMeta updateCategory);
        Task<ProductMeta> CreateProductCategory(ProductMeta createCategory);
        Task<HttpResponseMessage> DeleteProductCategory(string id);

        Task<IEnumerable<ProductMeta>> ListAllProductTags();
        Task<ProductMeta> GetProductTag(string id);
        Task<ProductMeta> UpdateProductTag(ProductMeta updateTag);
        Task<ProductMeta> CreateProductTag(ProductMeta createTag);
        Task<HttpResponseMessage> DeleteProductTag(string id);
    }
}
