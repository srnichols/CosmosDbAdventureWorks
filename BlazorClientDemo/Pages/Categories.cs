using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CosmosDbAdventureWorks.models;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using System.Net.Http;
using System.Net.Http.Json;

namespace BlazorClientDemo.Pages
{
    public partial class Categories
    {
        private ProductCategory[] myCategories;

        protected override async Task OnInitializedAsync()
        {
            myCategories = await Http.GetFromJsonAsync<ProductCategory[]>("sample-data/productCategory.json");
        }

    }
}
