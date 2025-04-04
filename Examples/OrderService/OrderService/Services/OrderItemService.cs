using Microsoft.EntityFrameworkCore;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using Steeltoe.Discovery;
using System.Text;

namespace OrderService.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IDiscoveryClient? _discoveryClient;
        private readonly HttpClient _client;

        public OrderItemService(IRepository<Order> orderRepository, IRepository<OrderItem> orderItemRepository, IDiscoveryClient discoveryClient, IHttpClientFactory httpClientFactory)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _discoveryClient = discoveryClient;
            _client = httpClientFactory.CreateClient();
        }
        public OrderItemService(IRepository<Order> orderRepository, IRepository<OrderItem> orderItemRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
   
        }

        public bool AddOrderItem(long orderId, OrderItemDTO orderItemDTO)
        {
            Order order = _orderRepository.GetById(orderId);
            if (order == null)
            {
                throw new ArgumentException("Order not found");
            }

            OrderItem orderItem;
            try
            {
                orderItem = new OrderItem
                {
                    ItemId = orderItemDTO.ItemId,
                    Quantity = orderItemDTO.Quantity,
                    Order = order,
                    Cost = getPriceAndReduceStock(orderItemDTO)
                };
            } catch (ArgumentException e)
            {
                throw new ArgumentException(e.Message);
            } catch (TimeoutException e)
            {
                throw new TimeoutException(e.Message);
            }


            _orderItemRepository.Insert(orderItem);
            return true;
        }

        public IEnumerable<OrderItem> GetOrderItems(long orderId)
        {
            Order order = _orderRepository.GetById(orderId, q => q.Include(o => o.OrderItems));
            if (order == null)
            {
                throw new ArgumentException("Order not found");
            }

            return order.OrderItems;
        }

        private int getPriceAndReduceStock(OrderItemDTO orderItem)
        {
            if (_discoveryClient == null)
            {
                return 0;
            }
            var instances = _discoveryClient.GetInstances("api-gateway");
            var instance = instances.FirstOrDefault();
            if (instance == null)
            {
                throw new TimeoutException("API-GATEWAY not found");
            }
            string baseUrl = $"http://{instance.Host}:{instance.Port}/api/v1/items/";
            
            string url = baseUrl + orderItem.ItemId + "/reserve";

            var requestBody = new { quantity = orderItem.Quantity };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = _client.PutAsync(url, httpContent);
            if (!response.Result.IsSuccessStatusCode)
            {
                throw new ArgumentException(response.Result.StatusCode.ToString());
            }
            var responseString = response.Result.Content.ReadAsStringAsync().Result;
            var item = Newtonsoft.Json.JsonConvert.DeserializeObject<ItemResponseDTO>(responseString);
            return item.Cost;
        }
    }
}
