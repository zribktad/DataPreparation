using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;
using Steeltoe.Discovery;
using Steeltoe.Discovery.Eureka.AppInfo;

namespace OrderService.Controllers
{

    [ApiController]
    [Route("api/v1/customers")]
    public class CustomerController : ControllerBase
    {

        private readonly ILogger<CustomerController> _logger;
        private readonly ICustomerService _customerService;

        public CustomerController(ILogger<CustomerController> logger, ICustomerService customerService)
        {
            _logger = logger;
            _customerService = customerService;
        }
      
        [HttpGet(Name = "GetCustomers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            IEnumerable<Customer> customers = _customerService.GetAllCustomers();
            return Ok(customers);
        }

        [HttpGet("{id}", Name = "GetCustomer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(long id)
        {
            try
            {
                Customer customer = _customerService.GetCustomerById(id);
                return Ok(customer);
            }
            catch (InvalidOperationException e)
            {
                return NotFound(e.Message);
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult Post([FromBody] CustomerDTO customer)
        {
            Customer newCustomer = _customerService.CreateCustomer(customer);
            return CreatedAtRoute("GetCustomer", new { id = newCustomer.Id }, newCustomer);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Put(long id, [FromBody] Customer customer)
        {
            try
            {
                Customer customer1 = _customerService.UpdateCustomer(id, customer);
                return Ok(customer1);
            }
            catch (InvalidOperationException e)
            {
                return NotFound(e.Message);
            }
        }   
    }
}
