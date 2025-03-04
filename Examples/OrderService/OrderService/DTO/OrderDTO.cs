using OrderService.Models;

namespace OrderService.DTO
{
    public class OrderDTO
    {
        public long CustomerId { get; set; }
        public IEnumerable<OrderItem> OrderItems { get; set; }
    }
}
