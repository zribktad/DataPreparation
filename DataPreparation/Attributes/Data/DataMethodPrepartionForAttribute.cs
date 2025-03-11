using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Data
{
    /// <summary>
    /// Attribute to specify the method for which data preparation is required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DataMethodPreparationForAttribute : Attribute
    {
        /// <summary>
        /// Gets the MethodInfo of the method for which data preparation is required.
        /// </summary>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// Gets the lifetime of the service.
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMethodPreparationForAttribute"/> class.
        /// </summary>
        /// <param name="baseTestClass">The type of the class containing the method.</param>
        /// <param name="methodName">The name of the method for which data preparation is required.</param>
        /// <param name="lifetime">The lifetime of the service. Default is <see cref="ServiceLifetime.Singleton"/>.</param>
        public DataMethodPreparationForAttribute(Type baseTestClass, string methodName, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            MethodInfo = baseTestClass.GetMethod(methodName) ?? throw new ArgumentNullException(nameof(methodName));
            Lifetime = lifetime;
        }
    }
}