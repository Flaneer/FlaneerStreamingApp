using System.Diagnostics;
using FlaneerMediaLib.VideoDataTypes;
using OfflinePacketSimulator;

namespace PartialFrameBufferProfiling;


/// <summary>
/// The code in here is a bit hacky, but it is simply a bunch of methods that can be easily shifted around so they can be easily profiled
/// </summary>
public class Program
{
    private static void Main(string[] args)
    {
        var offlinePacketBuffer = new OfflinePacketBuffer();
        
        //ProcessReceptionPacket(offlinePacketBuffer);
        //TransmissionFrameParse(offlinePacketBuffer);
        BufferFullFrame(offlinePacketBuffer);
        //BufferPartialFrame(offlinePacketBuffer);
        
        
    }

    private static void ProcessReceptionPacket(OfflinePacketBuffer offlinePacketBuffer)
    {
        for (int i = 0; i < 100; i++)
        {
            offlinePacketBuffer.UDPReceiver.ProcessReceivedPacket(offlinePacketBuffer.GetRandomPacket());
            offlinePacketBuffer.RefreshFrameBuffer();
        }
    }
    
    private static void TransmissionFrameParse(OfflinePacketBuffer offlinePacketBuffer)
    {
        for (int i = 0; i < 100; i++)
        {
            TransmissionVideoFrame.FromUDPPacket(offlinePacketBuffer.GetRandomPacket().Buffer);
            offlinePacketBuffer.RefreshFrameBuffer();
        }
    }
    
    private static void BufferFullFrame(OfflinePacketBuffer offlinePacketBuffer)
    {
        for (int i = 0; i < 100; i++)
        {
            var randomFullFrame = offlinePacketBuffer.GetRandomFullFrame();
            offlinePacketBuffer.FrameBuffer.BufferFullFrame(randomFullFrame.Item1, randomFullFrame.Item2);
            
            offlinePacketBuffer.RefreshFrameBuffer();
        }
    }
    
    private static void BufferPartialFrame(OfflinePacketBuffer offlinePacketBuffer)
    {
        for (int i = 0; i < 100; i++)
        {
            foreach (var partialFrame in offlinePacketBuffer.GetRandomBlockOfPartialFrames())
            {
                offlinePacketBuffer.FrameBuffer.BufferPartialFrame(partialFrame.Item1, partialFrame.Item2);
            }

            offlinePacketBuffer.RefreshFrameBuffer();
        }
    }
    
    private static void EmptyLoop(OfflinePacketBuffer offlinePacketBuffer)
    {
        for (int i = 0; i < 100; i++)
        {
            
            
            offlinePacketBuffer.RefreshFrameBuffer();
        }
    }
}
