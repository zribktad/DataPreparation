using System.Text.Json.Serialization;

namespace OrderService.Models
{
    public class BaseEntity
    {
        [JsonIgnore]
        public long Id { get; set; }
    }
}
