namespace DataPreparation.Data;

/// <summary>
/// Marks a method to be executed during the data preparation "up" phase before test execution.
/// Methods with this attribute are responsible for setting up test data required by tests.
/// </summary>
/// <remarks>
/// This attribute should be applied to methods within data preparation classes that:
/// 1. Create necessary database records
/// 2. Initialize required test objects
/// 3. Configure system state required for testing
/// 
/// UpData methods are executed before the test runs, and their corresponding DownData methods
/// (if defined) will be executed after the test completes, in reverse order.
/// 
/// Example usage:
/// <code>
/// public class OrderTestData
/// {
///     [UpData]
///     public Order CreateOrder()
///     {
///         var order = new Order { ... };
///         // Save to database, etc.
///         return order;
///     }
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class UpDataAttribute : Attribute
{
    // This is a marker attribute - no additional properties or methods required
}