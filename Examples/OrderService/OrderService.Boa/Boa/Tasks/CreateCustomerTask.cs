using Boa.Constrictor.Screenplay;
using OrderService.Boa.CustomerService.Abilities;
using OrderService.Boa.ShowCases;
using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Boa.CustomerService.Tasks;


//[PrepareData] //use attribute as identificator, mark class with data
public class CreateCustomerTask : ITask
{
    private readonly CustomerDTO _customerDto;
    public Customer CreatedCustomer { get; private set; }

    public CreateCustomerTask(CustomerDTO customerDto)
    {
        _customerDto = customerDto;
    }

    public void PerformAs(IActor actor)
    {
        var ability = actor.Using<UseCustomerService>();
        CreatedCustomer = ability.Service.CreateCustomer(_customerDto);
        
    }

    public static CreateCustomerTask For(CustomerDTO customerDto) => new CreateCustomerTask(customerDto);
    
    // [UpData]
    // public void UpData(){}
    //
    // [DownData]
    // public void DownData()
    // {
    // }
}