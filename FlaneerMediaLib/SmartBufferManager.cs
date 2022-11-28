namespace FlaneerMediaLib;

internal class SmartBufferManager : IService
{
    private List<SmartBuffer> buffers = new List<SmartBuffer>();

    private int nextBufferIdx = 0;

    private const int InitialBufferCount = 10;

    public SmartBufferManager()
    {
        for (int i = 0; i < InitialBufferCount; i++)
        {
            buffers.Add(new SmartBuffer());
        }
    }

    public SmartBuffer CheckoutNextBuffer()
    {
        var ret = buffers[nextBufferIdx];
        ret.CheckedOut = true;
        for (int i = 0; i < buffers.Count; i++)
        {
            if (!buffers[i].CheckedOut)
            {
                nextBufferIdx = i;
                break;
            }
        }
        return ret;
    }

    public void ReleaseBuffer(SmartBuffer buffer)
    {
        buffer.CheckedOut = false;
    }
    
    private void ExpandBuffers()
    {
        var upper = buffers.Count / 2;
        for (int i = 0; i < upper; i++)
        {
            buffers.Add(new SmartBuffer());
        }
    }
}
