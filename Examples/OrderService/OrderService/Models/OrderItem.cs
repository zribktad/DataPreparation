using System.Text.Json.Serialization;

namespace OrderService.Models
{
    public class OrderItem : BaseEntity
    {
        public int Cost { get; set; }   
        public long ItemId { get; set; }
        public int Quantity { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
    }
}
