using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorServerApp.Models;
using Microsoft.AspNetCore.Components;


namespace BlazorServerApp.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient httpClient;

        public CustomerService(HttpClient httpClient) 
        {
            this.httpClient = httpClient;
        }

        public async Task<Customer> CreateCustomer(Customer createCustomer)
        {
            try
            {
                return await httpClient.PostJsonAsync<Customer>($"api/UpsertCustomer/upsert", createCustomer);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<SalesOrder> CreateSalesOrder(SalesOrder createSalesOrder)
        {
            try
            {
                return await httpClient.PostJsonAsync<SalesOrder>($"api/UpsertSalesOrder/upsert", createSalesOrder);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<HttpResponseMessage> DeleteCustomer(string id)
        {
            return await httpClient.DeleteAsync($"api/DeleteCustomer/delete/{id}");
        }

        public async Task<HttpResponseMessage> DeleteSalesOrder(string id)
        {
            return await httpClient.DeleteAsync($"api/DeleteSalesOrder/delete/{id}");
        }

        public async Task<Customer> GetCustomer(string id)
        {
            try
            {
                return await httpClient.GetJsonAsync<Customer>($"api/GetCustomerById/{id}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<SalesOrder> GetSalesOrder(string id)
        {
            try
            {
                return await httpClient.GetJsonAsync<SalesOrder>($"api/GetSalesOrderById/{id}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<IEnumerable<Customer>> ListAllCustomers()
        {
            return await httpClient.GetJsonAsync<Customer[]>("api/ListAllCustomers");
        }

        public async Task<IEnumerable<SalesOrder>> ListAllSalesOrders()
        {
            return await httpClient.GetJsonAsync<SalesOrder[]>("api/ListAllSalesOrders");
        }

        public async Task<IEnumerable<SalesOrder>> ListAllSalesOrdersByCustomerId(string id)
        {
            try 
            {
                
                return await httpClient.GetJsonAsync<SalesOrder[]>($"api/ListAllSalesOrdersByCustomerId/{id}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<IEnumerable<CustomerHeader>> ListCustomerHeaderByLastNameSearch(string searchTerm)
        {
            try 
            { 
             return await httpClient.GetJsonAsync<CustomerHeader[]>($"api/ListCustomerHeaderByLastNameSearch/{searchTerm}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<Customer> UpdateCustomer(Customer updateCustomer)
        {
            try
            {
                return await httpClient.PostJsonAsync<Customer>($"api/UpsertCustomer/upsert", updateCustomer);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<SalesOrder> UpdateSalesOrder(SalesOrder updateSalesOrder)
        {
            try
            {
                return await httpClient.PostJsonAsync<SalesOrder>($"api/UpsertSalesOrder/upsert", updateSalesOrder);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }
    }
}
