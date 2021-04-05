using System;
using BlazorServerApp.Models;
using BlazorServerApp.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorServerApp.Pages
{
    public partial class TagEdit : ComponentBase
    {
        public ProductMeta myProductTag { get; set; } = new ProductMeta();
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
                PageHeaderText = "Edit Product Tag";
                PageHeaderNavUri = $"tagdetails/{Id}";

                myProductTag = await ProductMetaService.GetProductTag(Id);
                epochTime = myProductTag._ts;
                documentTime = new DateTime(1970, 1, 1).AddSeconds(epochTime);
                StateHasChanged();
            }
            else 
            {
                PageHeaderText = "New Product Tag";
                PageHeaderNavUri = $"tag/";

                myProductTag = new ProductMeta
                {
                    id = Guid.NewGuid().ToString(),
                    type = "tag"
                };
                StateHasChanged();
            }

        }

        protected async Task HandleValidSubmit()
        {
            ProductMeta result = null;

            if (Id != null)
            {
                result = await ProductMetaService.UpdateProductTag(myProductTag);
            }
            else
            {
                result = await ProductMetaService.CreateProductTag(myProductTag);
            }
            
            if (result != null)
            {
                NavigationManager.NavigateTo($"/tagdetails/{result.id}");
            }

        }
    }
}
