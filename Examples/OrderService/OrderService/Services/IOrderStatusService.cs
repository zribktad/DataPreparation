using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Services
{
    public interface IOrderStatusService
    {
        OrderStatusOutputDTO AddOrderStatus(long orderId, OrderStatusInputDTO statusDto);
        IEnumerable<OrderStatusOutputDTO> GetOrderStatuses(long orderId);

        AddressDTO GetSupplyAddress();
        void PostPackage(PackageDTO packageDTO);
    }
}
