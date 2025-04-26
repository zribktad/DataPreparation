namespace DataPreparation.Data.Setup;

/// <summary>
/// Interface for components that can asynchronously register and clean up test data objects.
/// This interface handles the asynchronous cleanup phase of the test data lifecycle.
/// </summary>
/// <remarks>
/// IDataRegisterAsync implementations are ideal for scenarios where test data cleanup involves:
/// 1. API calls to external services
/// 2. Database operations with async APIs
/// 3. Any I/O-bound operations that benefit from asynchronous execution
/// 
/// This interface is often implemented alongside IDataFactoryAsync to provide the complete
/// asynchronous lifecycle management for test data objects.
/// </remarks>
public interface IDataRegisterAsync : IDataFactoryBase
{
    /// <summary>
    /// Deletes a previously created test data object asynchronously.
    /// </summary>
    /// <param name="createId">The unique identifier that was provided during object creation</param>
    /// <param name="data">The object to delete</param>
    /// <param name="args">Optional parameters that were provided during object creation</param>
    /// <returns>A task that resolves to true if the object was successfully deleted; otherwise, false</returns>
    /// <remarks>
    /// This method should handle all aspects of test data cleanup, including:
    /// - Removing the object from databases or external systems asynchronously
    /// - Cleaning up any associated resources
    /// - Restoring the system state if needed
    /// 
    /// The createId and args parameters are passed from the creation operation,
    /// allowing implementations to maintain context between creation and deletion.
    /// </remarks>
    Task<bool> Delete(long createId, object data, IDataParams? args);
}

/// <summary>
/// Generic version of IDataRegisterAsync for strongly-typed test data objects.
/// </summary>
/// <typeparam name="T">The type of object this register handles</typeparam>
/// <remarks>
/// This generic interface provides type safety when asynchronously deleting test objects,
/// eliminating the need for type casting in the implementation.
/// </remarks>
public interface IDataRegisterAsync<T> : IDataRegisterAsync, IDataFactoryBase<T> where T : notnull
{
    /// <summary>
    /// Deletes a previously created strongly-typed test data object asynchronously.
    /// </summary>
    /// <param name="createId">The unique identifier that was provided during object creation</param>
    /// <param name="data">The strongly-typed object to delete</param>
    /// <param name="args">Optional parameters that were provided during object creation</param>
    /// <returns>A task that resolves to true if the object was successfully deleted; otherwise, false</returns>
    /// <remarks>
    /// This strongly-typed version eliminates the need for explicit type casting in the implementation,
    /// making the code more robust and less prone to runtime errors.
    /// 
    /// Example implementation:
    /// <code>
    /// public async Task&lt;bool&gt; Delete(long createId, Customer data, IDataParams? args)
    /// {
    ///     var client = _clientFactory.CreateClient("CustomerApi");
    ///     var response = await client.DeleteAsync($"/customers/{data.Id}");
    ///     return response.IsSuccessStatusCode;
    /// }
    /// </code>
    /// </remarks>
    Task<bool> Delete(long createId, T data, IDataParams? args);
    
    /// <summary>
    /// Implementation of the non-generic Delete method that delegates to the typed version.
    /// </summary>
    /// <exception cref="InvalidCastException">Thrown when the provided data object is not of type T</exception>
    Task<bool> IDataRegisterAsync.Delete(long createId, object data, IDataParams? args) => Delete(createId, (T)data, args);
}