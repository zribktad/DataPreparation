using DataPreparation.Factory.Testing;

namespace DataPreparation.Data.Setup;

/// <summary>
/// Core interface for factories that create test data objects synchronously.
/// Implementations of this interface are responsible for creating and deleting test data objects.
/// </summary>
/// <remarks>
/// IDataFactory implementations are the building blocks of the DataPreparation framework.
/// Each factory is responsible for:
/// 1. Creating a specific type of test object (e.g., Customer, Order)
/// 2. Persisting it to a data store if necessary (e.g., database, API)
/// 3. Providing deletion logic to clean up the created object after test execution
/// 
/// The synchronous nature of this interface makes it suitable for:
/// - In-memory object creation
/// - Database operations with synchronous APIs
/// - Simple test data that doesn't require async operations
/// 
/// Example implementation:
/// <code>
/// public class CustomerFactory : IDataFactory&lt;Customer&gt;
/// {
///     private readonly DbContext _dbContext;
///     
///     public CustomerFactory(DbContext dbContext)
///     {
///         _dbContext = dbContext;
///     }
///     
///     public Customer Create(long createId, IDataParams? args)
///     {
///         var customer = new Customer
///         {
///             Name = "Test Customer " + createId,
///             // Initialize other properties...
///         };
///         
///         _dbContext.Customers.Add(customer);
///         _dbContext.SaveChanges();
///         return customer;
///     }
///     
///     public bool Delete(long id, object data, IDataParams? args)
///     {
///         var customer = (Customer)data;
///         _dbContext.Customers.Remove(customer);
///         _dbContext.SaveChanges();
///         return true;
///     }
/// }
/// </code>
/// </remarks>
public interface IDataFactory : IDataRegister
{
    /// <summary>
    /// Creates a new test data object synchronously.
    /// </summary>
    /// <param name="createId">A unique identifier for this creation operation</param>
    /// <param name="args">Optional parameters to customize the object creation</param>
    /// <returns>The created test data object</returns>
    /// <remarks>
    /// The createId parameter is useful for:
    /// - Creating unique identifiers or names for test objects
    /// - Correlating created objects with their cleanup operations
    /// - Generating deterministic test data for reproducibility
    /// 
    /// The args parameter allows tests to customize the created objects with specific properties.
    /// For example, creating a Customer with a specific name or status.
    /// </remarks>
    object Create(long createId, IDataParams? args);
}

/// <summary>
/// Generic version of IDataFactory that creates strongly-typed test data objects synchronously.
/// </summary>
/// <typeparam name="T">The type of object this factory creates</typeparam>
/// <remarks>
/// This generic interface provides type safety when creating test objects, eliminating the need
/// for type casting in tests. It's the preferred interface for implementing data factories.
/// 
/// Example usage in a test:
/// <code>
/// // In a test method
/// var customer = PreparationContext.GetFactory().New&lt;Customer, CustomerFactory&gt;();
/// // No casting needed - customer is already of type Customer
/// </code>
/// </remarks>
public interface IDataFactory<T> : IDataRegister<T>, IDataFactory where T : notnull
{
    /// <summary>
    /// Creates a new strongly-typed test data object synchronously.
    /// </summary>
    /// <param name="createId">A unique identifier for this creation operation</param>
    /// <param name="args">Optional parameters to customize the object creation</param>
    /// <returns>The created test data object of type T</returns>
    new T Create(long createId, IDataParams? args);
    
    /// <summary>
    /// Implementation of the non-generic Create method that delegates to the typed version.
    /// </summary>
    object IDataFactory.Create(long id, IDataParams? args) => Create(id, args);
}

