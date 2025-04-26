namespace DataPreparation.Data.Setup;

/// <summary>
/// Interface for components that can register and clean up test data objects.
/// This interface is the synchronous counterpart for data deletion operations.
/// </summary>
/// <remarks>
/// IDataRegister implementations handle the cleanup phase of the test data lifecycle:
/// 1. Deleting objects from databases
/// 2. Removing resources from external systems
/// 3. Restoring system state to pre-test conditions
/// 
/// This interface is often implemented alongside IDataFactory to provide the complete
/// lifecycle management for test data objects.
/// </remarks>
public interface IDataRegister: IDataFactoryBase 
{ 
    /// <summary>
    /// Deletes a previously created test data object synchronously.
    /// </summary>
    /// <param name="createId">The unique identifier that was provided during object creation</param>
    /// <param name="data">The object to delete</param>
    /// <param name="args">Optional parameters that were provided during object creation</param>
    /// <returns>True if the object was successfully deleted; otherwise, false</returns>
    /// <remarks>
    /// This method should handle all aspects of test data cleanup, including:
    /// - Removing the object from databases or external systems
    /// - Cleaning up any associated resources
    /// - Restoring the system state if needed
    /// 
    /// The createId and args parameters are passed from the creation operation,
    /// allowing implementations to maintain context between creation and deletion.
    /// </remarks>
    bool Delete(long createId, object data, IDataParams? args);
}

/// <summary>
/// Generic version of IDataRegister for strongly-typed test data objects.
/// </summary>
/// <typeparam name="T">The type of object this register handles</typeparam>
/// <remarks>
/// This generic interface provides type safety when deleting test objects,
/// eliminating the need for type casting in the implementation.
/// </remarks>
public interface IDataRegister<T> : IDataRegister, IDataFactoryBase<T> where T : notnull
{ 
    /// <summary>
    /// Deletes a previously created strongly-typed test data object synchronously.
    /// </summary>
    /// <param name="createId">The unique identifier that was provided during object creation</param>
    /// <param name="data">The strongly-typed object to delete</param>
    /// <param name="args">Optional parameters that were provided during object creation</param>
    /// <returns>True if the object was successfully deleted; otherwise, false</returns>
    /// <remarks>
    /// This strongly-typed version eliminates the need for explicit type casting in the implementation,
    /// making the code more robust and less prone to runtime errors.
    /// </remarks>
    bool Delete(long createId, T data, IDataParams? args);
    
    /// <summary>
    /// Implementation of the non-generic Delete method that delegates to the typed version.
    /// </summary>
    /// <exception cref="InvalidCastException">Thrown when the provided data object is not of type T</exception>
    bool IDataRegister.Delete(long createId, object data, IDataParams? args) => Delete(createId, (T)data, args);
}