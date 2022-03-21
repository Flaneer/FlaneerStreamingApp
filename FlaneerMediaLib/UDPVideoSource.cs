using System.Net;
using System.Net.Sockets;

namespace FlaneerMediaLib
{
    public class UDPVideoSource : IVideoSource
    {
        private FrameSettings frameSettings;
        private ICodecSettings codecSettings;
        private VideoCodec codec;
        private readonly int listenPort;

        UdpClient listener;
        IPEndPoint groupEP;

        private TransmissionVideoFrame latestFrame;

        public UDPVideoSource(int listenPort)
        {
            this.listenPort = listenPort;
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

            latestFrame = new TransmissionVideoFrame
            {
                Codec = codec,
                Height = (short) frameSettings.Height,
                Width = (short) frameSettings.Width
            };
            return true;
        }

        public VideoFrame GetFrame()
        {
            byte[] encodedBytes = listener.Receive(ref groupEP);
            
            
            
            return latestFrame;
        }

        public void Dispose()
        {
            listener.Dispose();
        }
    }
}
