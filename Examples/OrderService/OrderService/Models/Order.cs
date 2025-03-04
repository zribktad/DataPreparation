using System.Text.Json.Serialization;

namespace OrderService.Models
{
    public class Order : BaseEntity
    {
        public long Id { get; set; }
        public DateTime OrderDate { get; set; }
        public long CustomerId { get; set; }
        [JsonIgnore]
        public virtual IEnumerable<OrderItem> OrderItems { get; set; }
        [JsonIgnore]
        public virtual IEnumerable<OrderStatus> OrderStatuses { get; set; } 
        public Complaint Complaint { get; set; }
        public Rating Rating { get; set; }
    }
}
