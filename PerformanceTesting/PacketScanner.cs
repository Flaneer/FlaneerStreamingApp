using FlaneerMediaLib.VideoDataTypes;

namespace PerformanceTesting;

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
            var packet = File.ReadAllBytes($"C:/Users/Tom/Code/FlaneerStreamingApp/TestResources/SamplePackets/packet{i}.bin");
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

    public void PopulateFullFrameList(List<Tuple<TransmissionVideoFrame, byte[]>> fullFrameData)
    {
        foreach (var fullFrame in fullFrames)
        {
            var packet = File.ReadAllBytes($"C:/Users/Tom/Code/FlaneerStreamingApp/TestResources/SamplePackets/packet{fullFrame}.bin");
            TransmissionVideoFrame tvf = TransmissionVideoFrame.FromUDPPacket(packet);
            fullFrameData.Add(new Tuple<TransmissionVideoFrame, byte[]>(tvf, packet));
        }
    }
    
    public void PopulatePartialFrameList(List<List<Tuple<TransmissionVideoFrame, byte[]>>> partialFrameData)
    {
        foreach (var partialFrame in partialFrames)
        {
            var newList = new List<Tuple<TransmissionVideoFrame, byte[]>>();
            partialFrameData.Add(newList);
            foreach (var frameIdx in partialFrame.Value)
            {
                var packet = File.ReadAllBytes($"C:/Users/Tom/Code/FlaneerStreamingApp/TestResources/SamplePackets/packet{frameIdx}.bin");
                TransmissionVideoFrame tvf = TransmissionVideoFrame.FromUDPPacket(packet);
                newList.Add(new Tuple<TransmissionVideoFrame, byte[]>(tvf, packet));
            }
        }
    }
}
