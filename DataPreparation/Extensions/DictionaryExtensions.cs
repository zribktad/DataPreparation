namespace DataPreparation.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TValue> newValueFactory) where TKey : notnull
    {
        lock (dictionary)
        {
            if (dictionary.TryGetValue(key, out TValue? existingValue))
            {
                return existingValue;
            }

            TValue newValue = newValueFactory();
            dictionary[key] = newValue;
            return newValue;
        }
    }
}