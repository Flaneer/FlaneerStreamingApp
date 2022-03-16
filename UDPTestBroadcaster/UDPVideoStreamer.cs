using FlaneerMediaLib;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace LocalMediaFileOut
{
    internal class UDPVideoStreamer : IVideoSink
    {
        IEncoder encoder;

        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPAddress broadcast = IPAddress.Parse("212.132.204.217");

        private const int UDPHEADERSIZE = 28;
        
        public UDPVideoStreamer()
        {
            if(ServiceRegistry.TryGetService<IEncoder>(out var encoder))
                this.encoder = encoder;
            else
                throw new Exception("No available encoder");
        }

        public unsafe void Capture(int numberOfFrames, int targetFramerate)
        {
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
                    using (UnmanagedMemoryStream ustream = new UnmanagedMemoryStream((byte*)frame.FrameData, frame.FrameSize))
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
        
                            IPEndPoint ep = new IPEndPoint(broadcast, 11000);
                            s.SendTo(bytes, ep);
                            Console.WriteLine($"SENT CHUNK | {sent}/{ustream.Length}");
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
