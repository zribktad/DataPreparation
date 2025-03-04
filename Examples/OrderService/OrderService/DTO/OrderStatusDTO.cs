namespace OrderService.DTO
{
    public class OrderStatusInputDTO
    {
        public string OrderStatus { get; set; }
    }
    public class OrderStatusOutputDTO
    {
        public string OrderStatus { get; set; }
        public DateTime StatusDate { get; set; }
    }
}
