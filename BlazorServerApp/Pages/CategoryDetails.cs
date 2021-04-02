using BlazorServerApp.Models;
using BlazorServerApp.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorServerApp.Pages
{
    public partial class CategoryDetails : ComponentBase
    {
        public ProductMeta myProductCategory { get; set; } = new ProductMeta();
        public long epochTime;
        public DateTime documentTime;

        [Inject]
        public IProductMetaService ProductMetaService { get; set;}
    
        [Parameter]
        public string Id { get; set; }

        protected async override Task OnInitializedAsync()
        {
            try 
            { 
                myProductCategory = await ProductMetaService.GetProductCategory(Id);
                epochTime = myProductCategory._ts;
                documentTime = new DateTime(1970, 1, 1).AddSeconds(epochTime);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
            }
        }

    }
}
