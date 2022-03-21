using System.Net;
using System.Net.Sockets;

namespace FlaneerMediaLib
{
    public class UDPVideoSource : IVideoSource
    {
        private FrameSettings frameSettings;
        private ICodecSettings codecSettings;
        private VideoCodec codec;

        UdpClient listener;
        IPEndPoint groupEP;

        private Dictionary<int, ManagedVideoFrame> frameBuffer = new();
        private Dictionary<int, byte[]> partialFrames = new();
        private byte nextframe = 0;

        public UDPVideoSource(int listenPort)
        {
            listener = new UdpClient(listenPort);
            groupEP = new IPEndPoint(IPAddress.Any, listenPort);
        }

        public ICodecSettings CodecSettings => codecSettings;

        public FrameSettings FrameSettings => frameSettings;

        public bool Init(FrameSettings frameSettings, ICodecSettings codecSettings)
        {
            this.frameSettings = frameSettings;
            this.codecSettings = codecSettings;
            switch (codecSettings)
            {
                case H264CodecSettings:
                    codec = VideoCodec.H264;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(codecSettings));
            }

            //Initialise dictionary, so it can be used as a pool
            for (int i = 0; i < byte.MaxValue; i++)
            {
                frameBuffer.Add(i, new ManagedVideoFrame());
            }
            
            BeginReceptionThread();
            
            return true;
        }

        private void BeginReceptionThread()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    byte[] receivedBytes = listener.Receive(ref groupEP);
                    var parsedBroadcast = TransmissionVideoFrame.FromUDPPacket(receivedBytes);
                    TransmissionVideoFrame receivedFrame = parsedBroadcast.Item1;
                    if (receivedFrame.NumberOfPackets == 1)
                        frameBuffer[receivedFrame.SequenceIDX] = ManagedFrameFromTransmission(receivedFrame);
                    else
                        BufferPartialFrame(receivedFrame);
                }
            });
        }

        private void BufferPartialFrame(TransmissionVideoFrame receivedFrame)
        {
            throw new NotImplementedException();
        }

        private ManagedVideoFrame ManagedFrameFromTransmission(TransmissionVideoFrame receivedFrame)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///
        /// <remarks>
        /// This must ensure we:
        /// - Assemble frames from multiple packets
        /// - Provide frames in order
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        public VideoFrame GetFrame()
        {
            var ret = frameBuffer[nextframe];
            IncrementNextFrame();
            return ret;
        }

        private void IncrementNextFrame()
        {
            if (nextframe + 1 > byte.MaxValue)
            {
                nextframe = 0;
            }
            else
            {
                nextframe++;
            }
        }
        
        public void Dispose()
        {
            listener.Dispose();
        }
    }
}
