using System.Text.Json.Serialization;

namespace OrderService.Models
{
    public class Customer : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Address Address { get; set; }
        [JsonIgnore]
        public virtual IEnumerable<Order> Orders { get; set; } = new List<Order>();
    }
}
