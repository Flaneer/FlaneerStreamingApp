namespace FlaneerMediaLib;

/// <summary>
/// A buffer with the ability to be checked out and so it can be reused in a pool
/// </summary>
public class SmartBuffer
{
    /// <summary>
    /// Whether this buffer has been checked out for use
    /// </summary>
    public bool CheckedOut;

    /// <summary>
    /// The length of the buffer content, the buffer will always be the max size so we need to specify this seperately
    /// </summary>
    public int Length;
    
    /// <summary>
    /// The buffer accessor where things are stored
    /// </summary>
    public byte[] Buffer => buffer;

    private byte[] buffer;
    
    /// <summary>
    /// ctor
    /// </summary>
    public SmartBuffer()
    {
        buffer = new byte[Int16.MaxValue];
    }

    /// <summary>
    /// Ctor used for tests
    /// </summary>
    /// <remarks>Are you using this not in a test. Don't. Don't do that.</remarks>
    internal SmartBuffer(byte[] bufferIn)
    {
        buffer = bufferIn;
    }
}
