using FlaneerMediaLib.Logging;
using FlaneerMediaLib.UnreliableDataChannel;
using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib.VideoStreaming
{
    /// <summary>
    /// A video source that gets video from a UDP channel
    /// </summary>
    public class UDPVideoSource : IVideoSource
    {
        private FrameSettings frameSettings = null!;
            private ICodecSettings codecSettings = null!;
            private VideoCodec codec;
            
            /// <inheritdoc />
            public ICodecSettings CodecSettings => codecSettings;
                
            /// <inheritdoc />
            public FrameSettings FrameSettings => frameSettings;
        
            private FrameBuffer frameBuffer;
        
            private Logger logger;
                
            /// <summary>
            /// ctor
            /// </summary>
            public UDPVideoSource()
            {
                logger = Logger.GetLogger(this);
            }
            
            
            /// <inheritdoc />
            public bool Init(FrameSettings frameSettingsIn, ICodecSettings codecSettingsIn)
            {
                frameSettings = frameSettingsIn;
                codecSettings = codecSettingsIn;
                switch (codecSettingsIn)
                {
                    case H264CodecSettings:
                        codec = VideoCodec.H264;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(codecSettingsIn));
                }
        
                frameBuffer = new FrameBuffer(codec);
        
                if (!ServiceRegistry.TryGetService(out UDPReceiver receiver))
                {
                    throw new Exception("No UDP Receiver");
                }
                    
                receiver.SubscribeToReceptionTraffic(PacketType.VideoStreamPacket, frameBuffer.BufferFrame);
        
                return true;
            }
        
            /// <inheritdoc />
            public bool GetFrame(out IVideoFrame frame)
            {
                return frameBuffer.GetNextFrame(out frame);
            }
        
            /// <inheritdoc />
            public void Dispose(){}
    }
}
