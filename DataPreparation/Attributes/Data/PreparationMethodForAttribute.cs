using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Data
{
    /// <summary>
    /// Attribute to specify the method for which data preparation is required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PreparationMethodForAttribute : Attribute
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
        /// Initializes a new instance of the <see cref="PreparationMethodForAttribute"/> class.
        /// </summary>
        /// <param name="baseTestClass">The type of the class containing the method.</param>
        /// <param name="methodName">The name of the method for which data preparation is required.</param>
        /// <param name="lifetime">The lifetime of the service. Default is <see cref="ServiceLifetime.Scoped"/>.</param>
        public PreparationMethodForAttribute(Type baseTestClass, string methodName, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            MethodInfo = baseTestClass.GetMethod(methodName) ?? throw new ArgumentNullException(nameof(methodName));
            Lifetime = lifetime;
        }
    }
}