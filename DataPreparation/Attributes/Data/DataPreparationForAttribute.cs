using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Data
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataPreparationForAttribute: Attribute
    {
        public ServiceLifetime Lifetime { get; }
        public Type ClassType { get; }
        public DataPreparationForAttribute(Type type, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            ClassType = type;
            Lifetime= lifetime;
        }
    }

}
