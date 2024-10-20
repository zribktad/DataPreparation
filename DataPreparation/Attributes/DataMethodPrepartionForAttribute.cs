using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DataPreparation.Testing
{
 
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DataMethodPreparationForAttribute : Attribute
    {
    
        public MethodInfo MethodInfo { get; }
        public ServiceLifetime Lifetime { get; }

        public DataMethodPreparationForAttribute(Type baseTestClass, string methodName, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            MethodInfo = baseTestClass.GetMethod(methodName) ?? throw new ArgumentNullException(nameof(methodName));
            Lifetime = lifetime;
        }
    }
}