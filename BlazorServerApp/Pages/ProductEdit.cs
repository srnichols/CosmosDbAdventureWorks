using System;
using BlazorServerApp.Models;
using BlazorServerApp.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductModel = BlazorServerApp.Models.Product;

namespace BlazorServerApp.Pages
{
    public partial class ProductEdit : ComponentBase
    {
        public ProductModel myProduct { get; set; } = new ProductModel();
        public long epochTime;
        public DateTime documentTime;
        public string PageHeaderText { get; set; }
        public string PageHeaderNavUri { get; set; }


        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string Id { get; set; }
        
        [Parameter]
        public string Pk { get; set; }

        protected async override Task OnInitializedAsync()
        {
            if (Id != null)
            {
                PageHeaderText = "Edit Product";
                PageHeaderNavUri = $"productdetails/{Id}/{Pk}";

                myProduct = await ProductService.GetProduct(Id,Pk);
                epochTime = myProduct._ts;
                documentTime = new DateTime(1970, 1, 1).AddSeconds(epochTime);
                StateHasChanged();
            }
            else 
            {
                PageHeaderText = "New Product";
                PageHeaderNavUri = $"product/";

                myProduct = new ProductModel
                {
                    id = Guid.NewGuid().ToString(),
                    categoryId = "category"
                };
                StateHasChanged();
            }

        }

        protected async Task HandleValidSubmit()
        {
            ProductModel result = null;

            if (Id != null)
            {
                result = await ProductService.UpdateProduct(myProduct);
            }
            else
            {
                result = await ProductService.CreateProduct(myProduct);
            }
            
            if (result != null)
            {
                NavigationManager.NavigateTo($"/productdetails/{result.id}/{result.categoryId}");
            }

        }
    }
}
