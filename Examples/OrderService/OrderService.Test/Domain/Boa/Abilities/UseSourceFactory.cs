using Boa.Constrictor.Screenplay;
using DataPreparation.Provider;
using DataPreparation.Testing.Factory;

namespace OrderService.Test.Domain.Boa.Abilities;

public class UseSourceFactory : IAbility
{
    public ISourceFactory Factory { get; }

    public UseSourceFactory(ISourceFactory factory)
    {
        Factory = factory;
    }

    public static IAbility FromDataPreparation()
    {
        return new UseSourceFactory(PreparationContext.GetFactory());
    }
}