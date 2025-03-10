

using System;
using System.Collections.Generic;

namespace DataPreparation.Exceptions;

internal class ExceptionAggregator
{
    private readonly List<Exception> _exceptions = new();

    /// <summary>
    /// Adds a new exception to the list.
    /// </summary>
    public Exception? Add(Exception? ex)
    {
        if (ex != null) _exceptions.Add(ex);
        return ex;
    }

    /// <summary>
    /// Throws an AggregateException if there are any recorded exceptions.
    /// </summary>
    public AggregateException? Get()
    {
        return _exceptions.Count > 0 ? new AggregateException(_exceptions) : null;
    }

    /// <summary>
    /// Returns true if there are recorded exceptions.
    /// </summary>
    public bool HasExceptions => _exceptions.Count > 0;
}