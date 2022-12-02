using FlaneerMediaLib;
using FlaneerMediaLib.UnreliableDataChannel;
using FlaneerMediaLib.VideoDataTypes;
using FlaneerMediaLib.VideoStreaming;

namespace OfflinePacketSimulator;

public class OfflinePacketBuffer
{
    public FrameBuffer FrameBuffer => fbH264;

    public UDPReceiver UDPReceiver => udpReceiver;
    
    private readonly FrameBuffer fbH264;
    
    private UDPReceiver udpReceiver = new UDPReceiver();
    
    private PacketScanner packetScanner = new PacketScanner();

    private List<byte[]> rawPackets = new List<byte[]>();
    
    private List<Tuple<TransmissionVideoFrame, SmartBuffer>> fullFrameData = new();
    
    private List<List<Tuple<TransmissionVideoFrame, SmartBuffer>>> partialFrameData = new();
    
    private Tuple<TransmissionVideoFrame, List<SmartBuffer>> firstFrame;
    
    private const int NumberOfPackets = 200;

    public OfflinePacketBuffer(bool seedFrameBuffer = false)
    {
        if (!ServiceRegistry.TryGetService(out SmartBufferManager sbm))
        {
            SmartStorageSubsystem.InitSmartStorage();
        }
        
        
        fbH264 = new FrameBuffer(VideoCodec.H264, false);
        for (int i = 0; i < NumberOfPackets; i++)
        {
            rawPackets.Add(File.ReadAllBytes(OfflinePacketAccess.GetPacket(i)));
        }

        packetScanner.PopulateFullFrameList(fullFrameData);
        packetScanner.PopulatePartialFrameList(partialFrameData);
        packetScanner.PopulateFirstFrame(out firstFrame);

        if(seedFrameBuffer)
            SeedFrameBuffer();
    }

    private void SeedFrameBuffer()
    {
        foreach (var frameData in fullFrameData)
        {
            fbH264.BufferFullFrame(frameData.Item1, frameData.Item2);
        }
        udpReceiver.SubscribeToReceptionTraffic(PacketType.VideoStreamPacket, delegate { });
    }
    
    public SmartBuffer GetRandomPacket()
    {
        Random rnd = new Random();
        int num = rnd.Next(rawPackets.Count);
        return new SmartBuffer(rawPackets[num]);
    }

    public void SeedFirstFrame()
    {
        var assembledFrame = PartialFrame.AssembleFrameImpl(firstFrame.Item1, firstFrame.Item2);
        FrameBuffer.NewFrameReady(0, assembledFrame, true);
    }
    
    public Tuple<TransmissionVideoFrame, SmartBuffer> GetRandomFullFrame()
    {
        Random rnd = new Random();
        int num = rnd.Next(fullFrameData.Count);
        return fullFrameData[num];
    }

    public List<Tuple<TransmissionVideoFrame, SmartBuffer>> GetRandomBlockOfPartialFrames()
    {
        Random rnd = new Random();
        int num = rnd.Next(partialFrameData.Count);
        return partialFrameData[num];
    }
}
