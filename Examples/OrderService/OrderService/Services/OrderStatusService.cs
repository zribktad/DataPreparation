using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using Steeltoe.Discovery;
using System.Text;

namespace OrderService.Services
{
    public class OrderStatusService : IOrderStatusService
    {
        private readonly IRepository<OrderStatus> _orderStatusRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IDiscoveryClient _discoveryClient;
        private readonly ICustomerService _customerService;

        public OrderStatusService(IRepository<OrderStatus> orderStatusRepository, IRepository<Order> orderRepository, IDiscoveryClient discoveryClient, ICustomerService customerService)
        {
            _orderStatusRepository = orderStatusRepository;
            _orderRepository = orderRepository;
            _discoveryClient = discoveryClient;
            _customerService = customerService;
        }

        public OrderStatusOutputDTO AddOrderStatus(long orderId, OrderStatusInputDTO statusDto)
        {
            Status status;
            try
            {
                status = (Status)Enum.Parse(typeof(Status), statusDto.OrderStatus);
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException("Invalid status");
            }

            Order order = _orderRepository.GetById(orderId, q => q.Include(o => o.OrderStatuses).Include(o => o.OrderItems));
            if (order == null)
            {
                throw new ArgumentException("Order not found");
            }

            OrderStatus orderStatus = new OrderStatus()
            {
                Status = status,
                StatusDate = DateTime.Now.ToUniversalTime()
            };

            orderStatus.Order = order;
            
            if (status == Status.DELIVERING)
            {
                Customer customer = _customerService.GetCustomerById(order.CustomerId);
                AddressDTO supplyAddress = GetSupplyAddress();
               
                DeliveryAddressDTO supplyAddressDTO = new DeliveryAddressDTO
                {
                    City = supplyAddress.City,
                    Street = supplyAddress.Street,
                    PostalCode = supplyAddress.ZipCode,
                    Country = supplyAddress.State,
                    Number = customer.Phone
                };
                DeliveryAddressDTO deliveryAddressDTO = new DeliveryAddressDTO
                {
                    City = customer.Address.City,
                    Street = customer.Address.Street,
                    PostalCode = customer.Address.PostalCode,
                    Country = "Czechia",
                    Number = customer.Phone,
                    PackageId = 0
                };
                PackageDTO packageDTO = new PackageDTO
                {
                    OrderId = (int)orderId,
                    Weight = order.OrderItems.Aggregate(order.OrderItems.Count(), (acc, item) => acc + item.Quantity),
                    DeliveryAddress = deliveryAddressDTO,
                    SupplyAddress = supplyAddressDTO
                };
                PostPackage(packageDTO);

            }
            var orderStatusOut = _orderStatusRepository.Insert(orderStatus);
            return new OrderStatusOutputDTO
            {
                OrderStatus = orderStatusOut.Status.ToString(),
                StatusDate = orderStatusOut.StatusDate
            };
        }

        public IEnumerable<OrderStatusOutputDTO> GetOrderStatuses(long orderId)
        {
            Order order = _orderRepository.GetById(orderId,q => q.Include(c => c.OrderStatuses));
            if (order == null)
            {
               throw new ArgumentException("Order not found");
            }

            return order.OrderStatuses.Select(status => new OrderStatusOutputDTO
            {
                OrderStatus = status.Status.ToString(),
                StatusDate = status.StatusDate
            }).ToList();
        }

        public void PostPackage(PackageDTO packageDTO)
        {
            if (_discoveryClient == null)
            {
                return;
            }
            var instances = _discoveryClient.GetInstances("api-gateway");
            var instance = instances.FirstOrDefault();
            if (instance == null)
            {
                throw new TimeoutException("API-GATEWAY not found");
            }
            string baseUrl = $"http://{instance.Host}:{instance.Port}/api/v1/packages";

            var client = new HttpClient();
            var response = client.PostAsync(baseUrl, new StringContent(JsonConvert.SerializeObject(packageDTO), Encoding.UTF8, "application/json")).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new TimeoutException("Error posting package");
            }
        }

        public AddressDTO GetSupplyAddress()
        {
            if (_discoveryClient == null)
            {
                throw new Exception("Discovery Client not found");
            }
            var instances = _discoveryClient.GetInstances("api-gateway");
            var instance = instances.FirstOrDefault();
            if (instance == null)
            {
                throw new TimeoutException("API-GATEWAY not found");
            }
            string baseUrl = $"http://{instance.Host}:{instance.Port}/api/v1/addresses/supply-address";

            var client = new HttpClient();
            var response = client.GetAsync(baseUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<AddressDTO>(content);
            }
            else
            {
                throw new TimeoutException("Error getting supply address");
            }
        }
    }

}
