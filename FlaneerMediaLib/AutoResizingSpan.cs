using System.Runtime.InteropServices;

namespace FlaneerMediaLib;

/// <summary>
/// A span that automatically resizes if it cannot fit the data
/// </summary>
public unsafe class AutoResizingByteBuffer
{
    private readonly byte* pointer;
    private int length = 0;

    private byte[] buffer;
    private MemoryStream stream;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pointer"></param>
    /// <param name="length"></param>
    public AutoResizingByteBuffer(byte* pointer, int length)
    {
        this.pointer = pointer;
        RefreshContent(length);
        stream = new MemoryStream(length);
    }

    /// <summary>
    /// Refreshes the content of the buffer using the new data at the pointer, it will resize the buffer if needed
    /// </summary>
    public void RefreshContent(int newLength)
    {
        if (newLength > length)
        {
            length = newLength;
            buffer = new byte[length];
            stream = new MemoryStream(length);
        }

        Marshal.Copy((IntPtr)pointer, buffer, 0, length);
    }

    /// <summary>
    /// Writes the bufer data into the stream
    /// </summary>
    public MemoryStream WriteToStream()
    {
        stream.Position = 0;
        stream.Write(buffer, 0, length);
        return stream;
    }
}

