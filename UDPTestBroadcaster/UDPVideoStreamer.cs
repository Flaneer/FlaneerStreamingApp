using FlaneerMediaLib;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.VideoDataTypes;

namespace LocalMediaFileOut
{
    internal class UDPVideoStreamer
    {
        readonly IEncoder? encoder;

        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private readonly IPAddress broadcast;
        private readonly int port;

        private const int UDPHEADERSIZE = 28;
        
        public UDPVideoStreamer()
        {
            if (ServiceRegistry.TryGetService<IEncoder>(out var encoderOut))
                encoder = encoderOut;
            
            ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clas);
            var frameSettings = clas.GetParams(CommandLineArgs.BroadcastAddress);
            broadcast = IPAddress.Parse(frameSettings[0]);
            port = Int32.Parse(frameSettings[1]);
        }

        public unsafe void Capture(int numberOfFrames, int targetFramerate)
        {
            if(encoder == null)
                return;
            
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            var frameTime = new TimeSpan(0, 0, (int)Math.Floor(1.0f/ targetFramerate));

            for (int i = 0; i < numberOfFrames; i++)
            {
                while (stopWatch.Elapsed < (frameTime*i))
                {
                    Thread.Sleep(1);
                }

                try
                {
                    var frame = encoder.GetFrame();
                    if (frame is UnmanagedVideoFrame unmanagedFrame)
                    {
                        using (UnmanagedMemoryStream ustream = new UnmanagedMemoryStream((byte*)unmanagedFrame.FrameData, unmanagedFrame.FrameSize))
                        {
                            int sent = 0;
                            while (sent <= ustream.Length)
                            {
                                var bodySize = 65536 - UDPHEADERSIZE;
                                var packetSize = Math.Min(bodySize, ustream.Length - sent);
                                // Read the source file into a byte array.
                                byte[] bytes = new byte[packetSize];
                                int numBytesToRead = (int)packetSize;
                                // Read may return anything from 0 to numBytesToRead.
                                int n = ustream.Read(bytes, 0, numBytesToRead);
    
                                // Break when the end of the file is reached.
                                if (n == 0)
                                    break;
    
                                sent += n;
            
                                IPEndPoint ep = new IPEndPoint(broadcast, port);
                                s.SendTo(bytes, ep);
                                Console.WriteLine($"SENT CHUNK | {sent}/{ustream.Length}");
                            }
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
            }
            stopWatch.Stop();
            
        }
    }
}
