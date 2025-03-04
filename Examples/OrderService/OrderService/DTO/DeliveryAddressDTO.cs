namespace OrderService.DTO
{
    public class DeliveryAddressDTO
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Number { get; set; }
        public int PackageId { get; set; }

    }
}
