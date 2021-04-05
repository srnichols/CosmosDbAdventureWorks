using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using ProductModel = BlazorServerApp.Models.Product;
using BlazorServerApp.Services;

namespace BlazorServerApp.Pages
{
    public partial class Product : ComponentBase
    {
        [Inject]
        public IProductService ProductService { get; set; }
        public IEnumerable<ProductModel> myProducts { get; set; }


        protected override async Task OnInitializedAsync()
        {
            await RefreshProductList();
        }

        private async Task RefreshProductList()
        {
            myProducts = (await ProductService.ListAllProducts()).ToList();
            StateHasChanged();
        }

    }
}
