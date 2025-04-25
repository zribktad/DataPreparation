using Boa.Constrictor.Screenplay;
using DataPreparation.Provider;
using DataPreparation.Testing.Factory;

namespace OrderService.Test.Domain.Boa.Abilities;

public class UseSourceFactory(ISourceFactory sFactory) : IAbility
{
    public ISourceFactory SFactory { get; } = sFactory;
    public static IAbility FromDataPreparation()
    {
        return new UseSourceFactory(PreparationContext.GetFactory());
    }
}