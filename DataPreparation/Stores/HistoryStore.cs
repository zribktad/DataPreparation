using System.Collections.Concurrent;
using System.Text;

namespace DataPreparation.Models.Data;


public class HistoryStore<TId,T> where TId : notnull
{
        //**** GLOBAL SEQUENCE COUNTER
        // Unique sequence number generator for each entry.
        private long _sequenceCounter = 0;

        //**** CONCURRENT DICTIONARY TO STORE HISTORY ITEMS WITH SEQUENCE AS KEY
        // Key: sequence number, Value: Tuple (original id, item)
        private readonly ConcurrentDictionary<long, (TId id, T item)> _history = new();

        //**** CONCURRENT DICTIONARY TO MAP ORIGINAL ID TO SEQUENCE NUMBER
        // Key: original id, Value: sequence number
        private readonly ConcurrentDictionary<TId, long> _idToSeq = new();

        //**** ADD ITEM
        // Adds a new item with a given id to the history.
        // Returns the unique sequence number assigned to this entry.
        public bool TryAdd(TId id, T item)
        {
            var seq = Interlocked.Increment(ref _sequenceCounter);
            if( _history.TryAdd(seq, (id, item)))
            {
                if (_idToSeq.TryAdd(id, seq)) 
                    return true;
                _history.TryRemove(seq, out _);
            }
            return false;
        }

        //**** GET ITEM BY SEQUENCE
        // Retrieves an item from the history by its sequence number.
        public T? GetBySequence(long seq)
        {
            if (_history.TryGetValue(seq, out var entry)) 
                return entry.item;
            return default;
        }

        //**** GET ITEM BY ORIGINAL ID
        // Retrieves an item from the history by its original id.
        public T? GetById(TId id)
        {
            if (_idToSeq.TryGetValue(id, out long seq) && _history.TryGetValue(seq, out var entry))
                return entry.item;
            return default;
        }

        //**** GET ALL ITEMS
        // Returns all items in the history, ordered by their sequence number (ascending).
        public IEnumerable<T> GetAll(out IList<TId> ids)
        {
            var allItems= _history.OrderBy(pair => pair.Key)
                           .Select(pair => (pair.Value.id, pair.Value.item)).ToList();
            ids = allItems.Select(pair => pair.id).ToList();
            return allItems.Select(pair => pair.item);
        }

        //**** GET LATEST ITEMS
        // Returns the latest 'n' items from the history (if fewer exist, returns all),
        // ordered by their sequence number (oldest to newest).
        public IEnumerable<T> GetLatest(int n, out IEnumerable<TId> ids)
        {
            var latestItems = _history.OrderByDescending(pair => pair.Key)
                .Take(n)
                .OrderBy(pair => pair.Key).ToList();

            ids = latestItems.Select(pair => pair.Value.id);
            return latestItems.Select(pair => pair.Value.item);
        }

        public bool IsEmpty() => _history.IsEmpty;
     

        //**** GET LATEST ITEM
        // Returns the latest item in the history and its id, if it exists.
        public bool GetLatest(out T? item, out TId? id)
        {
            if(!IsEmpty())
            {
                var latestEntry = _history.OrderByDescending(pair => pair.Key).FirstOrDefault();
                item = latestEntry.Value.item;
                id = latestEntry.Value.id;
                return true;
            }

            item = default;
            id = default;
            return false;
        }


        //**** CLEANUP ITEM
        // Removes an item from the history by its original id.
        // Returns true if the item was successfully removed.
        public bool CleanupItem(TId id)
        {
            if (_idToSeq.TryRemove(id, out long seq))
            {
                return _history.TryRemove(seq, out _);
            }
            return false;
        }

        //**** CLEANUP ALL
        // Clears the entire history.
        public void CleanupAll()
        {
            _history.Clear();
            _idToSeq.Clear();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Data in history:\n");
            foreach (var entry in _history)
            {
                sb.Append($"CreatedId: {entry.Value.id}, Item: {entry.Value.item}\n");
            }
            return sb.ToString();
        }
}