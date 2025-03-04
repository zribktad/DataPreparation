using System.Text.Json.Serialization;

namespace OrderService.Models
{
    public class Rating : BaseEntity
    {
        public int NumOfStars { get; set; }
        public string Reason { get; set; }
        [JsonIgnore]
        public long OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
    }
}
