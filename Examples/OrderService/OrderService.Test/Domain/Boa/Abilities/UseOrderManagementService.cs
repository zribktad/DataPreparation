using Boa.Constrictor.Screenplay;
using DataPreparation.Provider;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Services;

namespace OrderService.BoaTest.Boa.Abilities;

public class UseOrderManagementService : IAbility
{
    public IOrderManagementService Service { get; }

    private UseOrderManagementService(IOrderManagementService service)
    {
        Service = service;
    }

    public static UseOrderManagementService With(IOrderManagementService service)
    {
        return new UseOrderManagementService(service);
    }

    public static IAbility FromDataPreparationProvider()
    {
        return new UseOrderManagementService(PreparationContext.GetProvider()
            .GetRequiredService<IOrderManagementService>());
    }
}