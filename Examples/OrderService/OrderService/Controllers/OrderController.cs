using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.DTO;
using OrderService.Exceptions;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;
using System.Text;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
    public class OrderController : ControllerBase
    {

        private readonly ILogger<CustomerController> _logger;
        private readonly IOrderService _orderService;
        private readonly IOrderManagementService _orderManagementService;
        public readonly IOrderStatusService _orderStatusService;
        public readonly ICustomerService _customerService;
        public readonly IOrderItemService _orderItemService;

        public OrderController(ILogger<CustomerController> logger, IOrderService orderService, IOrderManagementService orderManagementService,
            IOrderStatusService orderStatusService, IOrderItemService orderItemService, ICustomerService customerService)
        {
            _logger = logger;
            _orderService = orderService;
            _orderManagementService = orderManagementService;
            _orderStatusService = orderStatusService;
            _orderItemService = orderItemService;
            _customerService = customerService;
        }

        [HttpGet(Name = "GetOrders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetOrders()
        {
            var orders = _orderService.GetOrders();
            return Ok(orders);
        }

        [HttpGet("{id}", Name = "GetOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetOrder(long id)
        {
            try
            {
                var order = _orderService.GetOrder(id);
                return Ok(order);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult PostOrder([FromBody] OrderDTO orderDTO)
        {
            if (orderDTO == null)
            {
                return BadRequest();
            }
            try
            {
                var newOrder = _orderService.CreateOrder(orderDTO);
                return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
            }
            catch (InvalidOperationException e)
            {
                return NotFound(e.Message);
            }
        }


        [HttpPost("{id}/rating")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult PostRating(long id, [FromBody] RatingDTO rating)
        {
            try
            {
                var newRating = _orderManagementService.AddRatingToOrder(id, rating);
                return CreatedAtRoute("GetOrder", new { id = newRating.OrderId }, newRating);
            }
            catch (InvalidOperationException e)
            {
                return NotFound(e.Message);
            }
            catch (AlreadyExistsException e)
            {
                return Conflict(e.Message);
            }
        }

        [HttpPost("{id}/complaint")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult PostComplaint(long id, [FromBody] ComplaintDTO complaint)
        {
            try
            {
                var newComplaint = _orderManagementService.AddComplaintToOrder(id, complaint);
                return CreatedAtRoute("GetOrder", new { id = newComplaint.OrderId }, newComplaint);
            }
            catch (InvalidOperationException e)
            {
                return NotFound(e.Message);
            }
            catch (AlreadyExistsException e)
            {
                return Conflict(e.Message);
            }
        }

        [HttpPut("{id}/complaint")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult PutComplaint(long id, [FromBody] ComplaintDTO complaint)
        {
            try
            {
                var updatedComplaint = _orderManagementService.UpdateComplaintStatus(id, complaint);
                return CreatedAtRoute("GetOrder", new { id = updatedComplaint.OrderId }, updatedComplaint);
            }
            catch (InvalidOperationException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddStatus(long id, [FromBody] OrderStatusInputDTO statusDTO)
        {
            try
            {
                var status = _orderStatusService.AddOrderStatus(id, statusDTO);
                return Ok(status);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetStatus(long id)
        {
            try
            {
                var orderStatuses = _orderStatusService.GetOrderStatuses(id);
                return Ok(orderStatuses);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost("{id}/items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult AddItem(long id, [FromBody] OrderItemDTO orderItemDTO)
        {
            try
            {
                var orderItem = _orderItemService.AddOrderItem(id, orderItemDTO);
                return Ok(orderItem);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
            catch (TimeoutException e)
            {
                return StatusCode(503, e.Message);
            }
        }

        [HttpGet("{id}/items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetItems(long id)
        {
            try
            {
                var orderItems = _orderItemService.GetOrderItems(id);
                return Ok(orderItems);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpGet("{id}/receipt")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetReceipt(long id)
        {
            try
            {
                StringBuilder receipt = new StringBuilder();
                var order = _orderService.GetOrder(id);
                var customer = _customerService.GetCustomerById(order.CustomerId);
                receipt.Append("Receipt for Order\n");
                receipt.Append("Customer Name: " + customer.Name + "\n");
                receipt.Append("Order ID: " + id + "\n");
                var orderItems = _orderItemService.GetOrderItems(id);
                foreach (OrderItem item in orderItems)
                {
                    receipt.Append("Item ID: " + item.ItemId + " - Quantity: " + item.Quantity + ", Cost: " + item.Cost + ", Total Cost: " + item.Cost*item.Quantity + "\n");
                }
                receipt.Append("Total Cost: " + orderItems.Sum(i => i.Cost * i.Quantity) + "\n");
                receipt.Append("Date created: " + DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy") + "\n");
                receipt.Append("Thank you for shopping with us!");
                return Ok(receipt.ToString());
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}
