using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NReco.VideoConverter;

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

        private ManagedVideoFrame latestFrame;

        public UDPVideoSource(int listenPort)
        {
            this.listenPort = listenPort;
            listener = new UdpClient(listenPort);
            groupEP = new IPEndPoint(IPAddress.Any, listenPort);
        }

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

            latestFrame = new ManagedVideoFrame
            {
                Codec = codec,
                Height = 1440,
                Width = 2560
            };
            return true;
        }

        public VideoFrame GetFrame()
        {
            byte[] encodedBytes = listener.Receive(ref groupEP);
            
            var outputStream = new MemoryStream(encodedBytes);
            
            var ffMpeg = new FFMpegConverter();
            try
            {
                var task = ffMpeg.ConvertLiveMedia(Format.h264, outputStream, Format.mjpeg, new ConvertSettings());
                task.Start();
                task.Write(encodedBytes, 0, encodedBytes.Length);
                task.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            latestFrame.Stream = outputStream;
            
            return latestFrame;
        }

        public void Dispose()
        {
            listener.Dispose();
        }
        
    }
}
