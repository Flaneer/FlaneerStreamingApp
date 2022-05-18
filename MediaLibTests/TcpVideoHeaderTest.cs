using System.Threading;
using FlaneerMediaLib;
using Xunit;

namespace MediaLibTests;

public class TcpVideoHeaderTest
{
    [Fact]
    public void TestPacketIsReceived()
    {
        /*
         * Local host ip address, local port (1 each for sink and source) 
         * publish one frame from the sink
         * receive the frame in source, assert the value is the same
         */
        var packet = new byte[] { 1, 2, 3, 4, 5 };
        var receivedBytes = new byte[5];
        var source = new VideoHeaderSource();
        var sink = new VideoHeaderSink();


        var waitingForReception = true;

        void OnSourceReceivedData(object sender, byte[] bytes)
        {
            receivedBytes = bytes;
            waitingForReception = false;
        }
        source.ReceivedData += OnSourceReceivedData;
        

        Assert.True(sink.SendData(packet));
        while (waitingForReception)
        {
            Thread.Sleep(1);
        }
        Assert.Equal(packet, receivedBytes);
        
    }
}