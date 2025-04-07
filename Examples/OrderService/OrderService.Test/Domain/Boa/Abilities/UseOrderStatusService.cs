using Boa.Constrictor.Screenplay;
using DataPreparation.Provider;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Services;

namespace OrderService.BoaTest.OrderStatusService.Abilities;

public class UseOrderStatusService : IAbility
{
    public IOrderStatusService Service { get; }

    public UseOrderStatusService(IOrderStatusService service)
    {
        Service = service;
    }

    public static UseOrderStatusService With(IOrderStatusService service)
    {
        return new UseOrderStatusService(service);
    }

    public static UseOrderStatusService FromDataPreparationProvider()
    {
        return new UseOrderStatusService(PreparationContext.GetProvider().GetRequiredService<IOrderStatusService>());
    }
}