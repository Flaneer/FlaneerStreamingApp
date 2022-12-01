using FlaneerMediaLib;
using FlaneerMediaLib.VideoDataTypes;

namespace OfflinePacketSimulator;

public class PacketScanner
{
    private List<int> fullFrames = new List<int>();
    private Dictionary<int, List<int>> partialFrames = new ();

    public PacketScanner()
    {
        ScanAllPackets();
    }

    private void ScanAllPackets()
    {
        for (int i = 0; i < 100; i++)
        {
            var packet = File.ReadAllBytes(OfflinePacketAccess.GetPacket(i));
            var tvf = TransmissionVideoFrame.FromUDPPacket(packet);
            if (tvf.NumberOfPackets == 1)
            {
                fullFrames.Add(i);
            }
            else
            {
                partialFrames.Add(i, new List<int>());
                for (int j = 0; j < tvf.NumberOfPackets; j++)
                {
                    partialFrames[i].Add(i+j);
                }
                i += tvf.NumberOfPackets-1;
            }
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
}
