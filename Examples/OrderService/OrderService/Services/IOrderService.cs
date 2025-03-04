using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Services
{
    public interface IOrderService
    {
        IEnumerable<Order> GetOrders();
        Order GetOrder(long id);
        Order CreateOrder(OrderDTO orderDTO);
        bool UpdateOrder(long id, Order updatedOrder);
    }
}
