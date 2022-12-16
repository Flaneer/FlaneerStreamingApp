using FlaneerMediaLib;
using FlaneerMediaLib.SmartStorage;
using FlaneerMediaLib.UnreliableDataChannel;
using FlaneerMediaLib.VideoDataTypes;
using FlaneerMediaLib.VideoStreaming;

namespace OfflinePacketSimulator;

public class OfflinePacketBuffer
{
    public FrameBuffer FrameBuffer => fbH264;

    public UDPReceiver UDPReceiver => udpReceiver;
    
    private FrameBuffer fbH264;
    
    private UDPReceiver udpReceiver = new UDPReceiver();
    
    private PacketScanner packetScanner = new PacketScanner();

    private List<byte[]> rawPackets = new List<byte[]>();
    
    private List<Tuple<TransmissionVideoFrame, SmartBuffer>> fullFrameData = new();
    
    private List<List<Tuple<TransmissionVideoFrame, SmartBuffer>>> partialFrameData = new();
    
    private Tuple<TransmissionVideoFrame, List<SmartBuffer>> firstFrame;
    
    private const int NumberOfPackets = 200;

    private SmartMemoryStreamManager smartMemoryStreamManager;

    public OfflinePacketBuffer(bool seedFrameBuffer = false)
    {
        if (!ServiceRegistry.TryGetService(out smartMemoryStreamManager))
        {
            SmartStorageSubsystem.InitSmartStorage();
            ServiceRegistry.TryGetService(out smartMemoryStreamManager);
            //The first GetStream is slow and messes up the benchmarking
            var initialBuffer = smartMemoryStreamManager.GetStream(Int16.MaxValue);
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

    public void RefreshFrameBuffer()
    {
        fbH264 = new FrameBuffer(VideoCodec.H264, false);
    }
    
    public void SeedFirstFrame()
    {
        var numberOfPackets = firstFrame.Item1.NumberOfPackets;
        int[] orderIdxs = Enumerable.Range(0, numberOfPackets).ToArray();
        var framePieces = firstFrame.Item2.Select((sb, i) => new { Key = i, Value = sb }).ToDictionary(x => x.Key, x => x.Value);
        
        var unassembledFrame = new PartialUnassembledFrame(VideoCodec.H264, firstFrame.Item1, framePieces, orderIdxs);
        FrameBuffer.NewFrameReady(firstFrame.Item1.SequenceIDX, unassembledFrame, firstFrame.Item1.IsIFrame);
    }
    
    public MemoryStream GetFirstFrameStream()
    {
        var frame = firstFrame.Item2[0];
        var stream = smartMemoryStreamManager.GetStream(frame.Buffer);
        return stream;
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
