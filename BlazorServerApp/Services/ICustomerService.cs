using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorServerApp.Models;

namespace BlazorServerApp.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> ListAllCustomers();
        Task<IEnumerable<CustomerHeader>> ListCustomerHeaderByLastNameSearch();
        Task<Customer> GetCustomer(string id);
        Task<Customer> UpdateCustomer(Customer updateCustomer);
        Task<Customer> CreateCustomer(Customer createCustomer);
        Task<HttpResponseMessage> DeleteCustomer(string id);

        Task<IEnumerable<SalesOrder>> ListAllSalesOrders();
        Task<IEnumerable<SalesOrder>> ListAllSalesOrdersByCustomerId();
        Task<SalesOrder> GetSalesOrder(string id);
        Task<SalesOrder> UpdateSalesOrder(SalesOrder updateSalesOrder);
        Task<SalesOrder> CreateSalesOrder(SalesOrder createSalesOrder);
        Task<HttpResponseMessage> DeleteSalesOrder(string id);
    }
}
