using DataPreparation.Data.Setup;
using DataPreparation.Factory.Testing;
using Moq;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;

namespace OrderService.BoaTest.ShowCases.Factories;

public class OrderMockRepositoryFactory :IDataFactory<IRepository<Order>>
{
    public IRepository<Order> Create(long id, IDataParams? args)
    {
        
        var mockOrderRepository = new Mock<IRepository<Order>>();
        mockOrderRepository.Setup(repo => repo.Insert(It.IsAny<Order>())).Returns<Order>( (order) => order);;
        
        if(args?.Find<OrderDTO>(out var orderDto) == true)
        {
            mockOrderRepository.Setup(repo => repo.GetById(It.IsAny<long>(), It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
                .Returns((long id, Func<IQueryable<Order>, IQueryable<Order>> _) =>
                    new Order { Id = id, CustomerId = orderDto.CustomerId, OrderItems = orderDto.OrderItems });
        }
        
        return mockOrderRepository.Object;
    }

    public bool Delete(long id, IRepository<Order> data, IDataParams? args)
    {
        return true;
    }
}