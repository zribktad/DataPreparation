using Boa.Constrictor.Screenplay;
using System.Collections.Generic;
using OrderService.Boa.CustomerService.Abilities;
using OrderService.Models;

namespace OrderService.Boa.CustomerService.Questions;

public class AllCustomers : IQuestion<IEnumerable<Customer>>
{
    public IEnumerable<Customer> RequestAs(IActor actor)
    {
        var ability = actor.Using<UseCustomerService>();
        return ability.Service.GetAllCustomers();
    }
}