using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Data;

/// <summary>
/// Marks a class as providing data preparation for a specific test class.
/// This attribute establishes a convention-based mapping between a test class and its data preparation logic.
/// </summary>
/// <remarks>
/// When applied to a class, this attribute registers the class as the default data preparation class
/// for all tests within the specified test class. The framework will automatically discover and use
/// this class when tests in the target class are executed with data preparation attributes.
/// 
/// This approach decouples test classes from their data preparation implementation, allowing for cleaner
/// test code and separation of concerns.
/// 
/// Example usage:
/// <code>
/// // Test class
/// public class CustomerTests
/// {
///     [Test]
///     [UsePreparedDataFor(typeof(CustomerTests))] // Uses data preparation defined in CustomerTestsDataPreparation
///     public void CreateCustomer_WithValidData_CreatesCustomer()
///     {
///         // Test code using prepared data
///     }
/// }
/// 
/// // Data preparation class
/// [PreparationClassFor(typeof(CustomerTests))]
/// public class CustomerTestsDataPreparation
/// {
///     [UpData]
///     public void SetupCustomerTestData() 
///     {
///         // Setup code that runs before CustomerTests
///     }
///     
///     [DownData]
///     public void CleanupCustomerTestData() 
///     {
///         // Cleanup code that runs after CustomerTests
///     }
/// }
/// </code>
/// 
/// The data preparation class can define multiple UpData and DownData methods, which will be executed
/// before and after the test methods in the associated test class, respectively.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class PreparationClassForAttribute : Attribute
{
    /// <summary>
    /// Gets the lifetime of the service in the dependency injection container.
    /// </summary>
    /// <remarks>
    /// Controls how the data preparation class is instantiated and managed by the DI container:
    /// - Singleton: One instance is created and reused for all tests
    /// - Scoped: A new instance is created for each test fixture
    /// - Transient: A new instance is created each time it's requested
    /// 
    /// In most cases, Scoped (the default) is appropriate for test data preparation classes.
    /// </remarks>
    public ServiceLifetime Lifetime { get; }

    /// <summary>
    /// Gets the type of the test class for which this class provides data preparation.
    /// </summary>
    /// <remarks>
    /// This establishes the relationship between the data preparation class and the test class.
    /// The framework uses this relationship to automatically discover and apply the correct
    /// data preparation when tests in the target class are executed.
    /// </remarks>
    public Type ClassType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PreparationClassForAttribute"/> class.
    /// </summary>
    /// <param name="type">The type of the test class for which data preparation is provided</param>
    /// <param name="lifetime">The dependency injection lifetime for this data preparation class. Default is Scoped.</param>
    /// <remarks>
    /// When marking a data preparation class with this attribute, you must specify the test class
    /// that it targets. Optionally, you can specify the service lifetime to control how instances
    /// are created and managed.
    /// </remarks>
    public PreparationClassForAttribute(Type type, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        ClassType = type;
        Lifetime = lifetime;
    }
}