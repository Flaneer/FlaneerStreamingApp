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
        ulong nextFrame = (currentFrameTotal + 1) % byte.MaxValue;
        
        return (byte)nextFrame;
    }

    private ulong CyclicalAsTotal(byte cyclical) => cyclical + (cyclical * count[cyclical]);

    public void Increment()
    {
        currentFrameTotal++;
        count[currentFrameTotal % byte.MaxValue]++;
    }

    public void SkipTo(byte skipTo)
    {
        ulong skipToAsTotal = CyclicalAsTotal(skipTo);
        for (ulong i = currentFrameTotal; i < skipToAsTotal; i++)
        {
            count[i % 255]++;
        }
    }

    public byte Max(byte a, byte b)
    {
        return (byte)(Math.Max(CyclicalAsTotal(a), CyclicalAsTotal(b)) % byte.MaxValue);
    }

    public byte Min(byte a, byte b)
    {
        return (byte)(Math.Min(CyclicalAsTotal(a), CyclicalAsTotal(b)) % byte.MaxValue);
    }

    public bool IsOlder(byte sequenceIDX)
    {
        return CyclicalAsTotal(sequenceIDX) < currentFrameTotal;
    }
    
    public bool IsNewer(byte sequenceIDX)
    {
        return CyclicalAsTotal(sequenceIDX) > currentFrameTotal;
    }
}