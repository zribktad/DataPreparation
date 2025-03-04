using Microsoft.EntityFrameworkCore;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;

namespace OrderService.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _customerRepository;

        public CustomerService(IRepository<Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public IEnumerable<Customer> GetAllCustomers()
        {
            return _customerRepository.GetAll(q => q.Include(c => c.Address));
        }

        public Customer GetCustomerById(long id)
        {
            Customer customer = _customerRepository.GetById(id, q => q.Include(c => c.Address));
            if (customer == null)
            {
                throw new InvalidOperationException("Customer not found");
            }
            return _customerRepository.GetById(id, q => q.Include(c => c.Address));
        }

        public Customer CreateCustomer(CustomerDTO customerDTO)
        {
            Customer newCustomer = new Customer();
            newCustomer.Address = customerDTO.Address;
            newCustomer.Name = customerDTO.Name;
            newCustomer.Email = customerDTO.Email;
            newCustomer.Phone = customerDTO.Phone;
            _customerRepository.Insert(newCustomer);
            return newCustomer;
        }

        public Customer UpdateCustomer(long id, Customer customer)
        {
            var entity = _customerRepository.GetById(id, q => q.Include(c => c.Address));
            if (entity != null)
            {
                entity.Name = customer.Name;
                entity.Email = customer.Email;
                entity.Phone = customer.Phone;
                if(customer.Address == null)
                {
                    entity.Address = null;
                }
                else
                {
                    entity.Address = new Address();
                    entity.Address.Street = customer.Address.Street;
                    entity.Address.City = customer.Address.City;
                    entity.Address.PostalCode = customer.Address.PostalCode;
                }
                return _customerRepository.Update(entity);
            }
            else
            {
                throw new InvalidOperationException("Customer not found");
            }
        }
    }
}
