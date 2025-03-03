using System.Collections.Concurrent;
using System.Text;

namespace DataPreparation.Models.Data;


public class HistoryStore<T> where T : notnull
{
    
        //**** CONCURRENT DICTIONARY TO STORE HISTORY ITEMS WITH SEQUENCE AS KEY
        // Key: sequence number, Value: Tuple (original id, item)
        private readonly ConcurrentDictionary<long, T> _history = new();

        //**** ADD ITEM
        // Adds a new item with a given id to the history.
        // Returns the unique sequence number assigned to this entry.
        public bool TryAdd(long id, T item)
        {
            return _history.TryAdd(id,  item);
        }

      
        //**** GET ITEM BY ORIGINAL ID
        // Retrieves an item from the history by its original id.
        public T? GetById(long id)
        {
            return _history.GetValueOrDefault(id);
        }

        //**** GET ALL ITEMS
        // Returns all items in the history, ordered by their sequence number (ascending).
        public IList<T> GetAll(out IList<long> ids)
        {
            var allItems = _history.OrderBy(pair => pair.Key).ToList();
            ids = allItems.Select(pair => pair.Key).ToList();
            return allItems.Select(pair => pair.Value).ToList();
        }

        //**** GET LATEST ITEMS
        // Returns the latest 'n' items from the history (if fewer exist, returns all),
        // ordered by their sequence number (oldest to newest).
        public IList<T> GetLatest(int n, out IList<long> ids)
        {
            var latestItems = _history.OrderByDescending(pair => pair.Key)
                .Take(n).ToList();
            ids = latestItems.Select(pair => pair.Key).ToList();
            return latestItems.Select(pair => pair.Value).ToList();
        }

        public bool IsEmpty() => _history.IsEmpty;
     

        //**** GET LATEST ITEM
        // Returns the latest item in the history and its id, if it exists.
        public bool GetLatest(out T? item, out long? id)
        {
            if(!IsEmpty())
            {
                var latestEntry = _history.OrderByDescending(pair => pair.Key).FirstOrDefault();
                item = latestEntry.Value;
                id = latestEntry.Key;
                return true;
            }

            item = default;
            id = default;
            return false;
        }
        
        
        //**** Merge all history stores according descending order of keys
        static IList<T> MergeLatest(IEnumerable<HistoryStore<T>> historyStores)
        {
            return historyStores.SelectMany(store => store._history).OrderByDescending(pair => pair.Key).Select(pair => pair.Value).ToList();
        }
      
        //**** CLEANUP ITEM
        // Removes an item from the history by its original id.
        // Returns true if the item was successfully removed.
        public bool CleanupItem(long id)
        {
            return _history.TryRemove(id, out _);
        }

        //**** CLEANUP ALL
        // Clears the entire history.
        public void CleanupAll()
        {
            _history.Clear();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Data:\n");
            foreach (var entry in _history)
            {
                sb.Append($"CreatedId: {entry.Key}, Item: {entry.Value}\n");
            }
            return sb.ToString();
        }
}