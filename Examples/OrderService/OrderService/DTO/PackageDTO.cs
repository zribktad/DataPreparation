namespace OrderService.DTO
{
    public class PackageDTO
    {
        public int OrderId { get; set; }
        public int Weight { get; set; }
        public DeliveryAddressDTO DeliveryAddress { get; set; }
        public DeliveryAddressDTO SupplyAddress { get; set; }
    }
}
