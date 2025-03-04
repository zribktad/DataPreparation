using Boa.Constrictor.Screenplay;
using OrderService.Boa.CustomerService.Abilities;
using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Boa.CustomerService.Tasks;

public class UpdateCustomerTask : ITask
{
    private readonly long _customerId;
    private readonly Customer _updatedCustomer;
    public Customer UpdatedCustomer { get; private set; }

    public UpdateCustomerTask(long customerId, Customer updatedCustomer)
    {
        _customerId = customerId;
        _updatedCustomer = updatedCustomer;
    }

    public void PerformAs(IActor actor)
    {
        var ability = actor.Using<UseCustomerService>();
        UpdatedCustomer = ability.Service.UpdateCustomer(_customerId, _updatedCustomer);
    }

    public static UpdateCustomerTask For(long customerId, Customer updatedCustomer) => new UpdateCustomerTask(customerId, updatedCustomer);
}