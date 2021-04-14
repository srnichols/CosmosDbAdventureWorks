using System;
using BlazorServerApp.Models;
using BlazorServerApp.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductModel = BlazorServerApp.Models.Product;
using ProductTag = BlazorServerApp.Models.Tag;

namespace BlazorServerApp.Pages
{
    public partial class ProductEdit : ComponentBase
    {
        public ProductModel myProduct { get; set; } = new ProductModel();
        public List<ProductMeta> myCategories { get; set; } = new List<ProductMeta>();
        public List<ProductMeta> myTags { get; set; } = new List<ProductMeta>();
        public ProductTag myTag { get; set; } = new ProductTag();

        public long epochTime;
        public DateTime documentTime;
        public string PageHeaderText { get; set; }
        public string PageHeaderNavUri { get; set; }


        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public IProductMetaService ProductMetaService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string Id { get; set; }
        
        [Parameter]
        public string Pk { get; set; }

        protected async override Task OnInitializedAsync()
        {
            //Load up collections for dropdown list
            myCategories = (await ProductMetaService.ListAllProductCategories()).ToList();
            myTags = (await ProductMetaService.ListAllProductTags()).ToList();

            //Check if new or edit page 
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

        protected private void AddTagToList(ChangeEventArgs e)
        {
            var item = myTags.Find(x => x.id == e.Value.ToString());
            ProductTag myTagItem = new ProductTag();
            myTagItem.id = item.id;
            myTagItem.name = item.name;
            
            // Only add new tag if not already in collection
            var i = myProduct.tags.Find(x => x.id == e.Value.ToString());
            if (i == null) 
            {
                myProduct.tags.Add(myTagItem);
                StateHasChanged();
            }
        }
        protected private void UpdateTagList(string id) 
        {
            var item = myProduct.tags.Find(x => x.id == id);
            myProduct.tags.Remove(item);
            StateHasChanged();
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
