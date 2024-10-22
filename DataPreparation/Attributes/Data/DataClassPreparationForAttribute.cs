using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Data
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataClassPreparationForAttribute: Attribute
    {
        public ServiceLifetime Lifetime { get; }
        public Type ClassType { get; }
        public DataClassPreparationForAttribute(Type type, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            ClassType = type;
            Lifetime= lifetime;
        }
    }

}
