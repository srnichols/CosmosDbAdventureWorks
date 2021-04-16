using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using BlazorServerApp.Models;
using CustomerModel = BlazorServerApp.Models.Customer;
using BlazorServerApp.Services;

namespace BlazorServerApp.Pages
{
    public partial class Customer : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        public IEnumerable<CustomerHeader> myCustomers { get; set; } = null;

        [Parameter]
        public string SearchKey { get; set; }

        public string SearchTerm { get; set; } = null;
        public string PageHeaderNavUri { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await RefreshCustomerHeaderList();
        }

        private async Task RefreshCustomerHeaderList()
        {
            try 
            {
                if (SearchTerm == null)
                {
                    SearchTerm = SearchKey + '%';
                }
                else 
                {
                    SearchTerm = SearchTerm + '%';
                }

                PageHeaderNavUri = $"customer/{SearchTerm}";

                myCustomers = (await CustomerService.ListCustomerHeaderByLastNameSearch(SearchTerm)).ToList();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
            }
        }

    }
}
