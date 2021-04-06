using BlazorServerApp.Models;
using BlazorServerApp.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductModel = BlazorServerApp.Models.Product;
using TagModel = BlazorServerApp.Models.Tag;


namespace BlazorServerApp.Pages
{
    public partial class ProductDetails : ComponentBase
    {
        public ProductModel myProduct { get; set; } = new ProductModel();
        public long epochTime = 0;
        public DateTime documentTime;

        [Inject]
        public IProductService ProductService { get; set;}
        
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Pk { get; set; }

        protected async override Task OnInitializedAsync()
        {
            try 
            { 
                myProduct = await ProductService.GetProduct(Id,Pk);
             
                if (myProduct != null)
                {
                    epochTime = myProduct._ts;
                    documentTime = new DateTime(1970, 1, 1).AddSeconds(epochTime);
                    StateHasChanged();
                }
                
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected async Task Delete_Click() 
        {
            await ProductService.DeleteProduct(Id, Pk);
            NavigationManager.NavigateTo($"/product/");
        }
    }
}
