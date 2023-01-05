using FlaneerMediaLib;
using FlaneerMediaLib.SmartStorage;
using FlaneerMediaLib.VideoDataTypes;

namespace OfflinePacketSimulator;

public class PacketScanner
{
    private List<int> fullFrames = new List<int>();
    private Dictionary<int, List<int>> partialFrames = new ();
    
    private Dictionary<int, List<int>> framesToPackets = new ();

    public PacketScanner(int packetCount)
    {
        ScanAllPackets(packetCount);
    }

    private void ScanAllPackets(int packetCount)
    {
        int frameIdx = 0;
        for (int i = 0; i < packetCount; i++)
        {
            var packet = File.ReadAllBytes(OfflinePacketAccess.GetPacket(i));
            var tvf = TransmissionVideoFrame.FromUDPPacket(packet);
            if (tvf.NumberOfPackets == 1)
            {
                fullFrames.Add(i);
                framesToPackets.Add(frameIdx, new List<int> {i});
            }
            else
            {
                partialFrames.Add(i, new List<int>());
                for (int j = 0; j < tvf.NumberOfPackets; j++)
                {
                    partialFrames[i].Add(i+j);
                }
                framesToPackets.Add(frameIdx, partialFrames[i]);
                i += tvf.NumberOfPackets-1;
            }
            frameIdx++;
        }
    }

    public void PopulateFirstFrame(out Tuple<TransmissionVideoFrame, List<SmartBuffer>> firstFrame)
    {
        var firstPacket = File.ReadAllBytes(OfflinePacketAccess.GetPacket(0));
        
        TransmissionVideoFrame tvf = TransmissionVideoFrame.FromUDPPacket(firstPacket);
        var frameList = new List<SmartBuffer>();
        var packetBytes = File.ReadAllBytes(OfflinePacketAccess.GetPacket(0));
        firstFrame = new (TransmissionVideoFrame.FromUDPPacket(packetBytes), frameList);
        for (int i = 0; i < tvf.NumberOfPackets; i++)
        {
            frameList.Add(new SmartBuffer(File.ReadAllBytes(OfflinePacketAccess.GetPacket(i))));
        }
    }
    
    public void PopulateFullFrameList(List<Tuple<TransmissionVideoFrame, SmartBuffer>> fullFrameData)
    {
        foreach (var fullFrame in fullFrames)
        {
            var packet = File.ReadAllBytes(OfflinePacketAccess.GetPacket(fullFrame));
            TransmissionVideoFrame tvf = TransmissionVideoFrame.FromUDPPacket(packet);
            fullFrameData.Add(new Tuple<TransmissionVideoFrame, SmartBuffer>(tvf, new SmartBuffer(packet)));
        }
    }
    
    public void PopulatePartialFrameList(List<List<Tuple<TransmissionVideoFrame, SmartBuffer>>> partialFrameData)
    {
        foreach (var partialFrame in partialFrames)
        {
            var newList = new List<Tuple<TransmissionVideoFrame, SmartBuffer>>();
            partialFrameData.Add(newList);
            foreach (var frameIdx in partialFrame.Value)
            {
                var packet = File.ReadAllBytes(OfflinePacketAccess.GetPacket(frameIdx));
                TransmissionVideoFrame tvf = TransmissionVideoFrame.FromUDPPacket(packet);
                newList.Add(new Tuple<TransmissionVideoFrame, SmartBuffer>(tvf, new SmartBuffer(packet)));
            }
        }
    }
    
    public MemoryStream GetFrame(int frameIdx)
    {
        var packetList = framesToPackets[frameIdx];
        List<byte> bytes = new ();
        var offset = 0;
        for (int i = 0; i < packetList.Count; i++)
        {
            var packetPiece = File.ReadAllBytes(OfflinePacketAccess.GetPacket(packetList[i]));
            bytes.AddRange(packetPiece);
            bytes.RemoveRange(offset, TransmissionVideoFrame.HeaderSize);
            offset = bytes.Count;
        }
        
        var ms = new MemoryStream(bytes.Count);
        ms.Write(bytes.ToArray(), 0, bytes.Count);
        return ms;
    }
}
