namespace DataPreparation.Data.Setup;

/// <summary>
/// Interface for data parameters.
/// </summary>
public interface IDataParams
{
    /// <summary>
    /// Finds an element of type <typeparamref name="T"/> that matches the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type of the element to find.</typeparam>
    /// <param name="result">The found element, or default if no element is found.</param>
    /// <param name="predicate">The predicate to match the element against.</param>
    /// <returns>True if an element is found; otherwise, false.</returns>
    bool Find<T>(out T? result, Func<T, bool>? predicate = null) => throw new NotImplementedException();
}