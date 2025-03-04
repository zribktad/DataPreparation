using System.Text.Json.Serialization;

namespace OrderService.Models
{
    public class OrderStatus : BaseEntity
    { 
        public DateTime StatusDate { get; set; }
        public Status Status { get; set; }
        public Order Order { get; set; }
    }
}
