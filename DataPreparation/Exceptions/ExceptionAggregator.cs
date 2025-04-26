namespace DataPreparation.Exceptions;

/// <summary>
/// Collects multiple exceptions that may occur during operations like data cleanup,
/// allowing the framework to continue processing even after errors occur.
/// </summary>
/// <remarks>
/// This class is particularly useful in cleanup or disposal scenarios where we want to:
/// 1. Continue trying to clean up resources even if some cleanup operations fail
/// 2. Report all failures at the end rather than failing on the first error
/// 3. Preserve the full context of what went wrong for debugging
/// </remarks>
internal class ExceptionAggregator
{
    /// <summary>
    /// The collection of exceptions that have been recorded.
    /// </summary>
    private readonly List<Exception> _exceptions = new();

    /// <summary>
    /// Adds a new exception to the collection.
    /// </summary>
    /// <param name="ex">The exception to add (may be null, in which case nothing happens)</param>
    /// <returns>An AggregateException containing all collected exceptions, or null if none exist</returns>
    public AggregateException? Add(Exception? ex)
    {
        if (ex != null) _exceptions.Add(ex);
        return Get();
    }
    
    /// <summary>
    /// Adds all inner exceptions from an AggregateException to the collection.
    /// This flattens the AggregateException to avoid nesting aggregates.
    /// </summary>
    /// <param name="aggregateException">The AggregateException whose inner exceptions should be added</param>
    /// <returns>An AggregateException containing all collected exceptions, or null if none exist</returns>
    public AggregateException? Add(AggregateException aggregateException)
    {
        _exceptions.AddRange(aggregateException.InnerExceptions);
        return Get();
    }
  
    /// <summary>
    /// Returns an AggregateException containing all the collected exceptions, or null if no exceptions exist.
    /// </summary>
    /// <returns>An AggregateException containing all collected exceptions, or null if none exist</returns>
    public AggregateException? Get()
    {
        return _exceptions.Count > 0 ? new AggregateException(_exceptions) : null;
    }

    /// <summary>
    /// Checks whether any exceptions have been collected.
    /// </summary>
    /// <value>True if there are one or more exceptions collected; otherwise, false.</value>
    public bool HasExceptions => _exceptions.Count > 0;

    public void Clear()
    {
        _exceptions.Clear();
    }
}