using System.Threading;
using FlaneerMediaLib;
using Xunit;

namespace MediaLibTests;

[Collection("Sequential")]
public class TcpVideoHeaderTest
{
    private static readonly string[] INPUT = new[] {$"-{CommandLineArgs.BroadcastAddress}", "127.0.0.1", "13000"};
    
    [Fact]
    public void TestPacketIsReceived()
    {
        ServiceRegistry.ClearRegistry();
        CommandLineArgumentStore.CreateAndRegister(INPUT);
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

        source.ReceivedData += (o, b) =>
        {
            receivedBytes = b;
            waitingForReception = false;
        }; 
        

        Assert.True(sink.SendData(packet));
        int i = 0;
        while (waitingForReception && i < 5000)
        {
            Thread.Sleep(1);
            i++;
        }
        Assert.Equal(packet, receivedBytes);
        
    }
}
