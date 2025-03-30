using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Data.Factory;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class FactoryLifetimeAttribute: Attribute
{
    public ServiceLifetime Lifetime { get; }

    public FactoryLifetimeAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }
}