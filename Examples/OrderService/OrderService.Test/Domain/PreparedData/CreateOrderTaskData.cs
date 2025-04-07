using DataPreparation.Data;
using Moq;
using OrderService.BoaTest.OrderService.Tasks;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Test.Domain.TestFakeModels;

namespace OrderService.Test.Domain.PreparedData;
// ReSharper disable once UnusedType.Global
[PreparationClassFor(typeof(CreateOrderTask))]
public class CreateOrderTaskData(
    Mock<IRepository<Customer>> mockCustomerRepository,
    Mock<IRepository<Order>> mockOrderRepository,
    CreationOrderDTO orderDto)
{
    [UpData]
    public void BeforeTest(long customerId, string customerName, int orderItemCount)
    {
        // create order items
        var orderItems = CreateOrderItems(orderItemCount);
        
        //create address and customer
        var customer = CreateCustomer(customerId, customerName);
        
        // Setup DTO
        orderDto.CustomerId = customer.Id;
        orderDto.OrderItems = orderItems;
        
        // Setup mock repositories
        SetupCustomerRepository(customer);
        SetupOrderRepository(orderDto);
    }
        
    private List<OrderItem> CreateOrderItems(int count)
        {
        var items = new List<OrderItem>();
        for (var i = 0; i < count; i++)
        {
            items.Add(new OrderItem
            {
                Id = i + 1,
                ItemId = i + 1,
                Quantity = i + 1
            });
        }
        return items;
    }
       
    private Customer CreateCustomer(long id, string name)
    {
        return new Customer
        {
            Id = id,
            Name = name,
            Address = new Address { City = "City", Street = "Street", PostalCode = "ZipCode" }
        };
    }
        
    private void SetupCustomerRepository(Customer customer)
    {
        mockCustomerRepository.Setup(repo =>
                repo.GetById(orderDto.CustomerId, It.IsAny<Func<IQueryable<Customer>, IQueryable<Customer>>>()))
            .Returns(customer);
    }
        
    private void SetupOrderRepository(OrderDTO oorderDto)
    {
        mockOrderRepository.Setup(repo => repo.Insert(It.IsAny<Order>()))
            .Returns<Order>((order) => order); 
            
        mockOrderRepository.Setup(repo => repo.GetById(It.IsAny<long>(), It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
            .Returns((long id, Func<IQueryable<Order>, IQueryable<Order>> _) =>
                new Order
                {
                    Id = id, 
                    CustomerId = oorderDto.CustomerId, 
                    OrderItems = oorderDto.OrderItems
                });
    }
}