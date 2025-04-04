using OrderService.Models;

namespace OrderService.DTO
{
    public class OrderDTO
    {
        public long CustomerId { get; set; }
        public IList<OrderItem> OrderItems { get; set; }
    }
}
