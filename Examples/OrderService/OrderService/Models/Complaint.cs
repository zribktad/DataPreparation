using System.Text.Json.Serialization;

namespace OrderService.Models
{
    public class Complaint : BaseEntity
    {
        [JsonIgnore]
        public long Id { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }
        [JsonIgnore]
        public long OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
    }
}
