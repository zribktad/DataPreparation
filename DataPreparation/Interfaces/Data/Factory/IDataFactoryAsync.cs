namespace DataPreparation.Data.Setup;

/// <summary>
/// Core interface for factories that create test data objects asynchronously.
/// Implementations of this interface are responsible for creating and deleting test data objects
/// using asynchronous operations.
/// </summary>
/// <remarks>
/// IDataFactoryAsync implementations are ideal for scenarios where test data creation involves:
/// 1. API calls to external services
/// 2. Database operations with async APIs
/// 3. Any I/O-bound operations that benefit from asynchronous execution
/// 
/// Each factory is responsible for:
/// - Creating a specific type of test object asynchronously
/// - Persisting it to a data store if necessary
/// - Providing asynchronous deletion logic to clean up the created object
/// 
/// Example implementation:
/// <code>
/// public class CustomerApiFactory : IDataFactoryAsync&lt;Customer&gt;
/// {
///     private readonly IHttpClientFactory _clientFactory;
///     
///     public CustomerApiFactory(IHttpClientFactory clientFactory)
///     {
///         _clientFactory = clientFactory;
///     }
///     
///     public async Task&lt;Customer&gt; Create(long createId, IDataParams? args, CancellationToken token = default)
///     {
///         var client = _clientFactory.CreateClient("CustomerApi");
///         
///         var customer = new Customer
///         {
///             Name = "Test Customer " + createId,
///             // Initialize other properties...
///         };
///         
///         var response = await client.PostAsJsonAsync("/customers", customer, token);
///         response.EnsureSuccessStatusCode();
///         
///         var createdCustomer = await response.Content.ReadFromJsonAsync&lt;Customer&gt;(cancellationToken: token);
///         return createdCustomer;
///     }
///     
///     public async Task&lt;bool&gt; Delete(long id, object data, IDataParams? args, CancellationToken token = default)
///     {
///         var customer = (Customer)data;
///         var client = _clientFactory.CreateClient("CustomerApi");
///         
///         var response = await client.DeleteAsync($"/customers/{customer.Id}", token);
///         return response.IsSuccessStatusCode;
///     }
/// }
/// </code>
/// </remarks>
public interface IDataFactoryAsync : IDataRegisterAsync
{
    /// <summary>
    /// Asynchronously creates a new test data object.
    /// </summary>
    /// <param name="createId">A unique identifier for this creation operation</param>
    /// <param name="args">Optional parameters to customize the object creation</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation</param>
    /// <returns>A task that resolves to the created test data object</returns>
    /// <remarks>
    /// This method should handle all aspects of creating a test object, including:
    /// - Initializing the object with appropriate test values
    /// - Assigning unique identifiers based on the createId
    /// - Persisting the object to external systems if needed
    /// - Applying any customizations specified in the args parameter
    /// 
    /// The operation should respect the cancellation token, allowing tests to cancel
    /// long-running data creation operations.
    /// </remarks>
    Task<object> Create(long createId, IDataParams? args, CancellationToken token = default);
}

/// <summary>
/// Generic version of IDataFactoryAsync that creates strongly-typed test data objects asynchronously.
/// </summary>
/// <typeparam name="T">The type of object this factory creates</typeparam>
/// <remarks>
/// This generic interface provides type safety when creating test objects, eliminating the need
/// for type casting in tests. It's the preferred interface for implementing asynchronous data factories.
/// 
/// Example usage in a test:
/// <code>
/// // In a test method
/// var customer = await PreparationContext.GetFactory().NewAsync&lt;Customer, CustomerApiFactory&gt;();
/// // No casting needed - customer is already of type Customer
/// </code>
/// </remarks>
public interface IDataFactoryAsync<T> : IDataRegisterAsync<T>, IDataFactoryAsync where T : notnull
{
    /// <summary>
    /// Asynchronously creates a new strongly-typed test data object.
    /// </summary>
    /// <param name="createId">A unique identifier for this creation operation</param>
    /// <param name="args">Optional parameters to customize the object creation</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation</param>
    /// <returns>A task that resolves to the created test data object of type T</returns>
    /// <remarks>
    /// Implementation of this method should follow the same principles as the non-generic version,
    /// but returns a strongly-typed object for better type safety in tests.
    /// </remarks>
    Task<T> Create(long createId, IDataParams? args, CancellationToken token = default);
    
    /// <summary>
    /// Implementation of the non-generic Create method that delegates to the typed version.
    /// This handles the proper awaiting of the task for both hot and cold tasks.
    /// </summary>
    async Task<object> IDataFactoryAsync.Create(long createId, IDataParams? args, CancellationToken token)
    {
        var task = Create(createId, args, token);
        return task.IsCompletedSuccessfully
            ? task.Result
            : await task.ConfigureAwait(false);
    }
}