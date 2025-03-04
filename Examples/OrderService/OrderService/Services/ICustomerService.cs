using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Services
{
    public interface ICustomerService
    {
        IEnumerable<Customer> GetAllCustomers();
        Customer GetCustomerById(long id);
        Customer CreateCustomer(CustomerDTO customerDTO);
        Customer UpdateCustomer(long id, Customer customer);
        
    }
}
