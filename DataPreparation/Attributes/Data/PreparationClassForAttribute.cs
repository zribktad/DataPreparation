using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Data;

/// <summary>
/// Attribute to specify the class for which data preparation is required.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PreparationClassForAttribute : Attribute
{
    /// <summary>
    /// Gets the lifetime of the service.
    /// </summary>
    public ServiceLifetime Lifetime { get; }

    /// <summary>
    /// Gets the type of the class for which data preparation is required.
    /// </summary>
    public Type ClassType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PreparationClassForAttribute"/> class.
    /// </summary>
    /// <param name="type">The type of the class for which data preparation is required.</param>
    /// <param name="lifetime">The lifetime of the service. Default is <see cref="ServiceLifetime.Singleton"/>.</param>
    public PreparationClassForAttribute(Type type, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        ClassType = type;
        Lifetime = lifetime;
    }
}