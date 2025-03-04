using Boa.Constrictor.Screenplay;
using System.Collections.Generic;
using OrderService.Boa.CustomerService.Abilities;
using OrderService.Models;

namespace OrderService.Boa.CustomerService.Questions;

public class CustomerById(long customerId) : IQuestion<Customer>
{
    public Customer RequestAs(IActor actor)
    {
        var ability = actor.Using<UseCustomerService>();
        return ability.Service.GetCustomerById(customerId);
    }
}