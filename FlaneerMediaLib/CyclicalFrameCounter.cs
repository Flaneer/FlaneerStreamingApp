namespace FlaneerMediaLib;

/// <summary>
/// Keeps a running total frame count with a single byte. This is used to keep the transmission packets smaller.
/// </summary>
public class CyclicalFrameCounter
{
    private uint[] count = new uint[byte.MaxValue];

    private ulong currentFrameTotal = 0;
    
    public CyclicalFrameCounter()
    {
        for (int i = 0; i < byte.MaxValue; i++)
            count[i] = 0;
    }

    /// <summary>
    /// Returns the current frame.
    /// </summary>
    public byte GetCurrent()
    {
        ulong nextFrame = (currentFrameTotal) % byte.MaxValue;
        
        return (byte)nextFrame;
    }
    
    /// <summary>
    /// Returns the next frame
    /// </summary>
    public byte GetNext()
    {
        ulong nextFrame = (currentFrameTotal + 1) % byte.MaxValue;
        
        return (byte)nextFrame;
    }

    private ulong CyclicalAsTotal(byte cyclical) => cyclical + (byte.MaxValue * count[cyclical]);

    /// <summary>
    /// Increases the frame total by one
    /// </summary>
    public void Increment()
    {
        currentFrameTotal++;
        count[currentFrameTotal % byte.MaxValue]++;
    }

    /// <summary>
    /// Increases the frame total by one
    /// </summary>
    public static CyclicalFrameCounter operator ++(CyclicalFrameCounter cfc)
    {
        cfc.Increment();
        return cfc;
    }

    /// <summary>
    /// Skips the frame count to the specified frame
    /// <remarks>This will cycle the count as needed</remarks>
    /// </summary>
    public void SkipTo(byte skipTo)
    {
        ulong skipToAsTotal = CyclicalAsTotal(skipTo);
        for (ulong i = currentFrameTotal; i < skipToAsTotal; i++)
        {
            count[i % 255]++;
        }
        currentFrameTotal = skipToAsTotal;
    }

    /// <summary>
    /// Returns which frame is the larger of the two in terms of the total frame count
    /// </summary>
    public byte Max(byte a, byte b)
    {
        return (byte)(Math.Max(CyclicalAsTotal(a), CyclicalAsTotal(b)) % byte.MaxValue);
    }

    /// <summary>
    /// Returns which frame is the smaller of the two in terms of the total frame count
    /// </summary>
    public byte Min(byte a, byte b)
    {
        return (byte)(Math.Min(CyclicalAsTotal(a), CyclicalAsTotal(b)) % byte.MaxValue);
    }

    /// <summary>
    /// Determines if the provided frame index is older than the current
    /// </summary>
    public bool IsOlder(byte sequenceIDX)
    {
        return CyclicalAsTotal(sequenceIDX) < currentFrameTotal;
    }
    
    /// <summary>
    /// Determines if the provided frame index is newer than the current
    /// </summary>
    public bool IsNewer(byte sequenceIDX)
    {
        return CyclicalAsTotal(sequenceIDX) > currentFrameTotal;
    }
}