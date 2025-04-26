namespace DataPreparation.Data.Setup;

/// <summary>
/// Defines the contract for parameter objects that customize the creation of test data.
/// IDataParams serves as a flexible container for passing configuration data to factory methods.
/// </summary>
/// <remarks>
/// IDataParams enables decoupling between test methods and data factories by providing:
/// 
/// 1. A standardized way to configure test data objects during creation
/// 2. Type-safe access to configuration values via the Find method
/// 3. Flexibility to pass multiple parameters of different types to factory methods
/// 
/// This interface is typically implemented by simple POCO (Plain Old CLR Object) classes
/// that hold configuration values specific to a particular factory.
/// 
/// Example implementation:
/// <code>
/// public class OrderParams : IDataParams
/// {
///     public int CustomerId { get; set; }
///     public DateTime OrderDate { get; set; } = DateTime.Now;
///     public List&lt;OrderItem&gt; Items { get; set; } = new();
///     public decimal Discount { get; set; }
///     
///     public bool Find&lt;T&gt;(out T? result, Func&lt;T, bool&gt;? predicate = null)
///     {
///         // Return CustomerId if requested type is int
///         if (typeof(T) == typeof(int) &amp;&amp; (predicate == null || ((Func&lt;int, bool&gt;)predicate)((dynamic)CustomerId)))
///         {
///             result = (T)(object)CustomerId;
///             return true;
///         }
///         
///         // Return OrderDate if requested type is DateTime
///         if (typeof(T) == typeof(DateTime) &amp;&amp; (predicate == null || ((Func&lt;DateTime, bool&gt;)predicate)((dynamic)OrderDate)))
///         {
///             result = (T)(object)OrderDate;
///             return true;
///         }
///         
///         // Return Items if requested type is List&lt;OrderItem&gt;
///         if (typeof(T) == typeof(List&lt;OrderItem&gt;) &amp;&amp; (predicate == null || ((Func&lt;List&lt;OrderItem&gt;, bool&gt;)predicate)((dynamic)Items)))
///         {
///             result = (T)(object)Items;
///             return true;
///         }
///         
///         // No match found
///         result = default;
///         return false;
///     }
/// }
/// </code>
/// 
/// Example usage in a test:
/// <code>
/// [Test]
/// public void CreateOrder_WithDiscount_CalculatesCorrectTotal()
/// {
///     // Set up test data with parameters
///     var customer = PreparationContext.GetFactory().New&lt;Customer, CustomerFactory&gt;();
///     var order = PreparationContext.GetFactory().New&lt;Order, OrderFactory&gt;(
///         new OrderParams 
///         { 
///             CustomerId = customer.Id,
///             Discount = 0.1m,
///             Items = new List&lt;OrderItem&gt; 
///             { 
///                 new() { ProductId = 1, Quantity = 2, UnitPrice = 10.99m },
///                 new() { ProductId = 2, Quantity = 1, UnitPrice = 24.99m }
///             }
///         });
///     
///     // Assert that discount was applied correctly
///     Assert.That(order.Total, Is.EqualTo(42.26m).Within(0.01));
/// }
/// </code>
/// </remarks>
public interface IDataParams
{
    /// <summary>
    /// Finds an element of type <typeparamref name="T"/> that matches the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type of the element to find</typeparam>
    /// <param name="result">When this method returns, contains the found element if found; otherwise, the default value for type T</param>
    /// <param name="predicate">Optional predicate to filter elements of type T. If null, returns the first element of type T</param>
    /// <returns>True if an element is found; otherwise, false</returns>
    /// <remarks>
    /// This method provides a standardized way for factories to extract typed configuration values from parameter objects.
    /// 
    /// Implementations can use various strategies:
    /// - Simple properties: Return properties that match the requested type
    /// - Collections: Search in collections for matching elements
    /// - Nested parameters: Search in child parameter objects
    /// - Calculated values: Compute and return values based on other properties
    /// 
    /// The default implementation throws NotImplementedException, requiring implementing classes to override it.
    /// </remarks>
    bool Find<T>(out T? result, Func<T, bool>? predicate = null) => throw new NotImplementedException();
}