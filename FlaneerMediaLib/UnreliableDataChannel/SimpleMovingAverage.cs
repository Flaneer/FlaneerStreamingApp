namespace FlaneerMediaLib;

internal class SimpleMovingAverage
{
    private readonly long k;
    private readonly long[] values;

    private long index = 0;
    private long sum = 0;

    public SimpleMovingAverage(long k)
    {
        if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k), "Must be greater than 0");

        this.k = k;
        values = new long[k];
    }

    public long Update(long nextInput)
    {
        // calculate the new sum
        sum = sum - values[index] + nextInput;

        // overwrite the old value with the new one
        values[index] = nextInput;

        // increment the index (wrapping back to 0)
        index = (index + 1) % k;

        // calculate the average
        return  sum / k;
    }
}
