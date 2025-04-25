using Boa.Constrictor.Screenplay;
using DataPreparation.Provider;
using Microsoft.Extensions.DependencyInjection;
using OrderService.BoaTest.ShowCases.Factories;
using OrderService.Services;

namespace OrderService.BoaTest.OrderService.Abilities;

public class UseOrderService(IOrderService service) : IAbility
{
    public IOrderService Service { get; } = service;
    
    public static UseOrderService FromDataPreparationProvider()
    {
        return new UseOrderService(PreparationContext.GetProvider().GetRequiredService<IOrderService>());
    }
    public static UseOrderService With(IOrderService service)
    {
        return new UseOrderService(service);
    }
    
    public static UseOrderService FromMockFactory()
    {
        return new UseOrderService(PreparationContext.GetFactory().New<Services.OrderService, OrderServiceFactory>());
    }
}