using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorServerApp.Models;

namespace BlazorServerApp.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> ListAllProducts();
        Task<Product> GetProduct(string id, string pk);
        Task<Product> UpdateProduct(Product updateProduct);
        Task<Product> CreateProduct(Product createProduct);
        Task<HttpResponseMessage> DeleteProduct(string id, string pk);

    }
}
