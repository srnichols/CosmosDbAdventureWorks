using System;
using BlazorServerApp.Models;
using BlazorServerApp.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorServerApp.Pages
{
    public partial class CategoryEdit : ComponentBase
    {
        public ProductMeta myProductCategory { get; set; } = new ProductMeta();
        public long epochTime;
        public DateTime documentTime;
        public string PageHeaderText { get; set; }
        public string PageHeaderNavUri { get; set; }


        [Inject]
        public IProductMetaService ProductMetaService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string Id { get; set; }

        protected async override Task OnInitializedAsync()
        {
            if (Id != null)
            {
                PageHeaderText = "Edit Product Category";
                PageHeaderNavUri = $"categorydetails/{Id}";

                myProductCategory = await ProductMetaService.GetProductCategory(Id);
                epochTime = myProductCategory._ts;
                documentTime = new DateTime(1970, 1, 1).AddSeconds(epochTime);
                StateHasChanged();
            }
            else 
            {
                PageHeaderText = "New Product Category";
                PageHeaderNavUri = $"category/";

                myProductCategory = new ProductMeta
                {
                    id = Guid.NewGuid().ToString(),
                    type = "category"
                };
                StateHasChanged();
            }

        }

        protected async Task HandleValidSubmit()
        {
            ProductMeta result = null;

            if (Id != null)
            {
                result = await ProductMetaService.UpdateProductCategory(myProductCategory);
            }
            else
            {
                result = await ProductMetaService.CreateProductCategory(myProductCategory);
            }
            
            if (result != null)
            {
                NavigationManager.NavigateTo($"/categorydetails/{result.id}");
            }

        }
    }
}
