using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Stores and manages the relationships between test classes/methods and their corresponding data preparation classes.
    /// This registry enables convention-based test data preparation by maintaining mappings established via the PreparationClassFor
    /// and PreparationMethodFor attributes.
    /// </summary>
    /// <remarks>
    /// DataRelationStore acts as a central registry that maps:
    /// 1. Test classes to their corresponding data preparation classes
    /// 2. Test methods to their corresponding data preparation classes
    /// 
    /// These mappings are used by the framework when a test is executed with data preparation attributes but
    /// without explicit data preparation type references. This enables a more maintainable approach where
    /// test code doesn't need to directly reference data preparation implementation details.
    /// 
    /// The relationships are typically established during test assembly scanning and setup:
    /// - PreparationClassForAttribute establishes a class-level mapping
    /// - PreparationMethodForAttribute establishes a method-level mapping
    /// </remarks>
    internal abstract class DataRelationStore
    {
        /// <summary>
        /// Gets the data preparation type associated with a test class.
        /// </summary>
        /// <param name="classType">The test class type to find a data preparation type for</param>
        /// <returns>The data preparation type, or null if no mapping exists</returns>
        /// <remarks>
        /// This method is used by the framework when resolving implicit data preparation for a test class.
        /// The mapping is typically established using the PreparationClassForAttribute.
        /// 
        /// Example mapping:
        /// <code>
        /// [PreparationClassFor(typeof(CustomerTests))]
        /// public class CustomerTestsDataPreparation
        /// {
        ///     [UpData]
        ///     public void SetupCustomerTestData() { ... }
        ///     
        ///     [DownData]
        ///     public void CleanupCustomerTestData() { ... }
        /// }
        /// </code>
        /// </remarks>
        public static Type? GetClassDataPreparationType(Type classType)
        {
            return ClassDataRegister.GetValueOrDefault(classType);
        }
        
        /// <summary>
        /// Gets the data preparation type associated with a test method.
        /// </summary>
        /// <param name="methodInfo">The test method to find a data preparation type for</param>
        /// <returns>The data preparation type, or null if no mapping exists</returns>
        /// <remarks>
        /// This method is used by the framework when resolving implicit data preparation for a test method.
        /// The mapping is typically established using the PreparationMethodForAttribute.
        /// 
        /// Example mapping:
        /// <code>
        /// [PreparationMethodFor(typeof(CustomerTests), nameof(CustomerTests.CreateCustomer_WithValidData_CreatesCustomer))]
        /// public class CreateCustomerDataPreparation
        /// {
        ///     [UpData]
        ///     public void SetupCreateCustomerTestData() { ... }
        ///     
        ///     [DownData]
        ///     public void CleanupCreateCustomerTestData() { ... }
        /// }
        /// </code>
        /// </remarks>
        public static Type? GetMethodDataPreparationType(MethodInfo methodInfo)
        {
            return MethodDataRegister.GetValueOrDefault(methodInfo);
        }
        
        /// <summary>
        /// Checks if a test method has an associated data preparation type.
        /// </summary>
        /// <param name="methodInfo">The test method to check</param>
        /// <returns>True if the method has an associated data preparation type; otherwise, false</returns>
        /// <remarks>
        /// This method is used to quickly check if method-level data preparation is available
        /// without retrieving the actual preparation type.
        /// </remarks>
        public static bool HasMethodDataPreparationType(MethodInfo methodInfo)
        {
            return MethodDataRegister.ContainsKey(methodInfo);
        }

        /// <summary>
        /// Registers a data preparation type for a test class.
        /// </summary>
        /// <param name="classType">The test class type to register a data preparation for</param>
        /// <param name="data">The data preparation type to associate with the test class</param>
        /// <remarks>
        /// This method is typically called during test assembly scanning when PreparationClassForAttribute
        /// attributes are discovered.
        /// </remarks>
        public static void SetClassDataPreparationType(Type classType, Type data)
        {
           ClassDataRegister[classType] = data;
        }
        
        /// <summary>
        /// Registers a data preparation type for a test method.
        /// </summary>
        /// <param name="methodInfo">The test method to register a data preparation for</param>
        /// <param name="data">The data preparation type to associate with the test method</param>
        /// <remarks>
        /// This method is typically called during test assembly scanning when PreparationMethodForAttribute
        /// attributes are discovered.
        /// </remarks>
        public static void SetMethodDataPreparationType(MethodInfo methodInfo, Type data)
        {
            MethodDataRegister[methodInfo] = data;
        }
        
        /// <summary>
        /// Thread-safe dictionary mapping test classes to their data preparation types
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Type> ClassDataRegister = new();
        
        /// <summary>
        /// Thread-safe dictionary mapping test methods to their data preparation types
        /// </summary>
        private static readonly ConcurrentDictionary<MethodInfo, Type> MethodDataRegister = new();
    }
}
