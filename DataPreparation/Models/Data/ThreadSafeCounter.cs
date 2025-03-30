namespace DataPreparation.Models.Data;

public class ThreadSafeCounter
{
    private long _counter = 0;

    public long GetNextId()
    {
        return Interlocked.Increment(ref _counter);
    }

    public long GetCount()
    {
        return Interlocked.CompareExchange(ref _counter, 0, 0);
    }
}