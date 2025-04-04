using Boa.Constrictor.Screenplay;

namespace OrderService.BoaTest.CustomerService.Abilities;

public class UseCustomerService : IAbility
{
    public Services.ICustomerService Service { get; }

    public UseCustomerService(Services.ICustomerService service)
    {
        Service = service;
    }

    public static UseCustomerService With(Services.ICustomerService service)
    {
        return new UseCustomerService(service);
    }
}