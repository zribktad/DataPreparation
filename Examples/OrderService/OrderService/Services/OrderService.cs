using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using Steeltoe.Discovery;
using Steeltoe.Discovery.Eureka;
using System.Text;

namespace OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Customer> _customerRepository;

        public OrderService(IRepository<Order> orderRepository, IRepository<Customer> customerRepository )
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
           
        }
        
        public IEnumerable<Order> GetOrders()
        {
            return _orderRepository.GetAll(q => q.Include(o => o.Complaint).Include(o => o.Rating));
        }

        public Order GetOrder(long id)
        {
            var order = _orderRepository.GetById(id, q => q.Include(o => o.Complaint).Include(o => o.Rating));
            if (order == null)
            {
                throw new ArgumentException("Order not found");
            }
            return order;
        }

        public Order CreateOrder(OrderDTO orderDTO)
        {
            if (orderDTO == null)
            {
                throw new ArgumentNullException(nameof(orderDTO));
            }
            var customer = _customerRepository.GetById(orderDTO.CustomerId);
            if (customer == null)
            {
                throw new InvalidOperationException("Customer not found");
            }
            if(orderDTO.OrderItems == null || orderDTO.OrderItems.Count() == 0)
            {
                throw new ValidationException("Order items not found");
            }

            Order newOrder = new Order
            {
                CustomerId = orderDTO.CustomerId,
                OrderStatuses = [new OrderStatus { Status = Status.CREATED, StatusDate = DateTime.Now }],
                OrderItems = orderDTO.OrderItems.ToList(),
                OrderDate = DateTime.Now
            };
            
            return _orderRepository.Insert(newOrder);;
        }

        public bool UpdateOrder(long id, Order updatedOrder)
        {

            var existingOrder = _orderRepository.GetById(id);
            if (existingOrder == null)
            {
                throw new ArgumentException("Order not found");
            }

            existingOrder = updatedOrder;
            _orderRepository.Update(existingOrder);
            return true;
        }

      
    }
}

