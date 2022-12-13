namespace FlaneerMediaLib.SmartStorage;

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
        nextBufferIdx = -1;
        for (int i = 0; i < buffers.Count; i++)
        {
            if (!buffers[i].CheckedOut)
            {
                nextBufferIdx = i;
                break;
            }
        }
        
        if(nextBufferIdx == -1)
            nextBufferIdx = ExpandBuffers();
        
        return ret;
    }

    public void ReleaseBuffer(SmartBuffer buffer)
    {
        buffer.CheckedOut = false;
    }
    
    private int ExpandBuffers()
    {
        var ret = buffers.Count;
        var upper = ret / 2;
        for (int i = 0; i < upper; i++)
        {
            buffers.Add(new SmartBuffer());
        }
        return ret;
    }
}
