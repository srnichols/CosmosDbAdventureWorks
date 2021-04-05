﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using ProductMeta = BlazorServerApp.Models.ProductMeta;
using BlazorServerApp.Services;

namespace BlazorServerApp.Pages
{
    public partial class Tag : ComponentBase
    {
        [Inject]
        public IProductMetaService ProductMetaService { get; set; }
        public IEnumerable<ProductMeta> myTags { get; set; }


        protected override async Task OnInitializedAsync()
        {
            await RefreshTagList();
        }

        private async Task RefreshTagList()
        {
            myTags = (await ProductMetaService.ListAllProductTags()).ToList();
            StateHasChanged();
        }

    }
}
