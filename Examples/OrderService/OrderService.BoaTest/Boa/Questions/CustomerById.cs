using Boa.Constrictor.Screenplay;
using System.Collections.Generic;
using OrderService.BoaTest.CustomerService.Abilities;
using OrderService.Models;

namespace OrderService.BoaTest.CustomerService.Questions;

public class CustomerById(long customerId) : IQuestion<Customer>
{
    public Customer RequestAs(IActor actor)
    {
        var ability = actor.Using<UseCustomerService>();
        return ability.Service.GetCustomerById(customerId);
    }
}