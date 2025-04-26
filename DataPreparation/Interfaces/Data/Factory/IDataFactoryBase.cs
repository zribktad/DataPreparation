namespace DataPreparation.Data.Setup;

/// <summary>
/// Base interface for all data factory and data register interfaces in the DataPreparation framework.
/// </summary>
/// <remarks>
/// IDataFactoryBase serves as a marker interface at the root of the DataPreparation factory hierarchy:
/// 
/// IDataFactoryBase
/// ├── IDataFactory (synchronous object creation)
/// │   └── IDataFactory&lt;T&gt; (strongly-typed synchronous creation)
/// ├── IDataFactoryAsync (asynchronous object creation)
/// │   └── IDataFactoryAsync&lt;T&gt; (strongly-typed asynchronous creation)
/// ├── IDataRegister (synchronous object deletion)
/// │   └── IDataRegister&lt;T&gt; (strongly-typed synchronous deletion)
/// └── IDataRegisterAsync (asynchronous object deletion)
///     └── IDataRegisterAsync&lt;T&gt; (strongly-typed asynchronous deletion)
/// 
/// This interface hierarchy allows for common treatment of all factory and register components
/// while maintaining separation between different functional aspects (creation vs. deletion) 
/// and execution models (synchronous vs. asynchronous).
/// </remarks>
public interface IDataFactoryBase 
{
    // Marker interface - no members required
}

/// <summary>
/// Generic version of the base interface for data factories and data registers that operate on a specific type.
/// </summary>
/// <typeparam name="T">The type that the factory or register operates on</typeparam>
/// <remarks>
/// This generic marker interface establishes type information at the base level of the factory hierarchy,
/// which is then propagated through the more specific interfaces like IDataFactory&lt;T&gt; and IDataRegister&lt;T&gt;.
/// 
/// By establishing the type constraint at this level, the framework can perform type-based lookups
/// and operations against factories and registers without knowing their specific implementation interfaces.
/// </remarks>
public interface IDataFactoryBase<T> : IDataFactoryBase where T : notnull
{
    // Generic marker interface - no members required
}