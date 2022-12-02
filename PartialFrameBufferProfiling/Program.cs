using OfflinePacketSimulator;

namespace PartialFrameBufferProfiling;

public class Program
{
    private static void Main(string[] args)
    {
        var offlinePacketBuffer = new OfflinePacketBuffer();
        foreach (var partialFrame in offlinePacketBuffer.GetRandomBlockOfPartialFrames())
        {
            offlinePacketBuffer.FrameBuffer.BufferPartialFrame(partialFrame.Item1, partialFrame.Item2);
        }
    }
}
