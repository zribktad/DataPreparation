namespace DataPreparation.Models.Data;

public class ThreadSafeCounter
{
    private int _counter = 0;

    public int Increment()
    {
        return Interlocked.Increment(ref _counter);
    }

    public int GetCount()
    {
        return Interlocked.CompareExchange(ref _counter, 0, 0);
    }
}