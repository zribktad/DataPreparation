using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Services
{
    public interface IOrderItemService
    {
        bool AddOrderItem(long orderId, OrderItemDTO orderItemDTO);
        IEnumerable<OrderItem> GetOrderItems(long orderId);
    }
}
