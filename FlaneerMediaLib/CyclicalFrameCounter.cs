namespace FlaneerMediaLib;

public class CyclicalFrameCounter
{
    private uint[] count = new uint[byte.MaxValue];

    private ulong currentFrameTotal = 0;
    
    public CyclicalFrameCounter()
    {
        for (int i = 0; i < byte.MaxValue; i++)
            count[i] = 0;
    }

    public byte GetNext()
    {
        ulong nextFrame = currentFrameTotal % byte.MaxValue;
        if (nextFrame + 1 > byte.MaxValue)
            nextFrame = 0;
        else
            nextFrame++;

        count[nextFrame]++;
        currentFrameTotal++;
        
        return (byte)nextFrame;
    }

    private ulong cycLicalAsTotal(byte cyclical) => cyclical + (cyclical * count[cyclical]);

    public void Increment() => currentFrameTotal++;

    public void SkipTo(byte skipTo)
    {
        ulong skipToAsTotal = cycLicalAsTotal(skipTo);
        for (ulong i = currentFrameTotal; i < skipToAsTotal; i++)
        {
            count[i % 255]++;
        }
    }

    public byte Max(byte a, byte b)
    {
        return (byte)(Math.Max(cycLicalAsTotal(a), cycLicalAsTotal(b)) % byte.MaxValue);
    }

    public byte Min(byte a, byte b)
    {
        return (byte)(Math.Min(cycLicalAsTotal(a), cycLicalAsTotal(b)) % byte.MaxValue);
    }
}