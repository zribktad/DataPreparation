using Boa.Constrictor.Screenplay;
using System.Collections.Generic;
using OrderService.BoaTest.CustomerService.Abilities;
using OrderService.Models;

namespace OrderService.BoaTest.CustomerService.Questions;

public class AllCustomers : IQuestion<IEnumerable<Customer>>
{
    public IEnumerable<Customer> RequestAs(IActor actor)
    {
        
        var ability = actor.Using<UseCustomerService>();
        return ability.Service.GetAllCustomers();
    }
}