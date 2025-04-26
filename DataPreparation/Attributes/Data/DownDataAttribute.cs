namespace DataPreparation.Data;

/// <summary>
/// Marks a method to be executed during the data preparation "down" phase after test execution.
/// Methods with this attribute are responsible for cleaning up test data that was created
/// during the "up" phase.
/// </summary>
/// <remarks>
/// This attribute should be applied to methods within data preparation classes that:
/// 1. Delete database records created for the test
/// 2. Release acquired resources
/// 3. Reset system state after testing
/// 
/// DownData methods are executed after the test completes, in reverse order from their
/// corresponding UpData methods. This ensures proper cleanup even when test execution fails.
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
///     
///     [DownData]
///     public void DeleteOrder(Order order)
///     {
///         // Delete order from database, etc.
///     }
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class DownDataAttribute : Attribute
{
    // This is a marker attribute - no additional properties or methods required
}