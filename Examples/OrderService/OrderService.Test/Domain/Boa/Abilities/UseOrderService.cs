using Boa.Constrictor.Screenplay;
using DataPreparation.Provider;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Services;

namespace OrderService.BoaTest.OrderService.Abilities;

public class UseOrderService(Services.IOrderService service) : IAbility
{
    public Services.IOrderService Service { get; } = service;
    public static UseOrderService With(Services.IOrderService service) => new UseOrderService(service);

    public static UseOrderService FromDataPreparationProvider()
    {
        return new UseOrderService(PreparationContext.GetProvider().GetRequiredService<IOrderService>());
    }
}