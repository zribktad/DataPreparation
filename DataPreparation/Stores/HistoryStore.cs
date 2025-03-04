using System.Collections;
using System.Collections.Concurrent;
using System.Text;

namespace DataPreparation.Models.Data;

/// <summary>
/// A thread-safe history store that allows adding items, retrieving by ID, and retrieving the latest items.
/// </summary>
/// <typeparam name="T">The type of the item stored in the history store.</typeparam>
public class HistoryStore<T> where T : notnull
{
    
    /// <summary>
    /// The dictionary used to store items by their unique ID.
    /// </summary>
    private readonly ConcurrentDictionary<long, HistoryItem<T>> _itemsById = new();

    /// <summary>
    /// The stack used to maintain the order of items for popping the latest.
    /// </summary>
    private readonly ConcurrentStack<HistoryItem<T>> _stack = new();
    


    /// <summary>
    /// Adds a new item to the history store.
    /// </summary>
    /// <param name="id">The unique identifier of the item to add.</param>
    /// <param name="item">The item to add to the history store.</param>
    public bool TryAdd(long id, T item)
    {
        var historyItem = new HistoryItem<T>(id, item);
        if (_itemsById.TryAdd(id, historyItem))
        {
            _stack.Push(historyItem);  
            return true;
        }
        return false;
    }

  
    /// <summary>
    /// Retrieves an item from the history store by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the item to retrieve.</param>
    /// <returns>The item associated with the given ID, or null if not found.</returns>
    public T? GetById(long id)
    {
        return _itemsById.TryGetValue(id, out var item) ? item.Value : default;
    }

    /// <summary>
    /// Retrieves the latest <paramref name="count"/> number of items from the history store and their IDs.
    /// </summary>
    /// <param name="count">The number of latest items to retrieve.</param>
    /// <param name="items">A list of the latest items in the history store.</param>
    /// <param name="ids">A list of IDs associated with the latest items.</param>
    public bool TryGetLatest(int count, out List<T> items, out List<long> ids)
    {
        items = new List<T>();
        ids = new List<long>();

        if (_stack.IsEmpty)
            return false;

        var latestItems = _stack.Take(count).ToList();
        items = latestItems.Select(item => item.Value).ToList();
        ids = latestItems.Select(item => item.Id).ToList();
        
        return count == items.Count;
    }
    
    /// <summary>
    /// Retrieves the latest item from the history store and its ID.
    /// </summary>
    /// <param name="item">The latest item in the history store.</param>
    /// <param name="id">The ID associated with the latest item.</param>
    public bool TryGetLatest(out T? item, out long? id)
    {
        item = default;
        id = default;

        if (_stack.IsEmpty)
            return false;

        if (_stack.TryPeek(out var latestItem))
        {
            item = latestItem.Value;
            id = latestItem.Id;
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Pops the last item from the history store and removes it.
    /// </summary>
    /// <returns>The value of the popped item, or null if the stack is empty.</returns>
    public T? Pop()
    {
        if (_stack.TryPop(out var item))
        {
            _itemsById.TryRemove(item.Id, out _); // Removing item by ID
            return item.Value;
        }
        return default; // If the stack is empty
    }

    public IList<T> GetAll(out IList<long> ids)
    {
        var sortedItems = _itemsById.OrderBy(pair => pair.Key).ToList();
        ids = sortedItems.Select(pair => pair.Key).ToList();
        return sortedItems.Select(pair => pair.Value.Value).ToList();
    }

    
    //**** CLEANUP ALL
    // Clears the entire history.
    public void CleanupAll()
    {
        _stack.Clear();
        _itemsById.Clear();
    }
    

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("Remaining data:\n");
        foreach (var entry in _stack)
        {
            sb.Append($"{entry.Value.ToString()}\n");
        }
        return sb.ToString();
    }
    
}

/// <summary>
/// Represents an item in the history, containing an ID and a value.
/// </summary>
/// <typeparam name="T">The type of the value stored in the history item.</typeparam>
public class HistoryItem<T> where T : notnull
{
    /// <summary>
    /// Gets the ID of the history item.
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// Gets the value of the history item.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryItem{T}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the history item.</param>
    /// <param name="value">The value associated with the history item.</param>
    public HistoryItem(long id, T value)
    {
        Id = id;
        Value = value;
    }

    /// <summary>
    /// Returns a string representation of the history item.
    /// </summary>
    /// <returns>A string that represents the current history item.</returns>
    public override string ToString() => $"[{Id}] {Value}";
}