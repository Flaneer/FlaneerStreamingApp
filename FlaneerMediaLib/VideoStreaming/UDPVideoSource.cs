using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.Logging;
using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib
{
    /// <summary>
    /// A video source that gets video from a UDP channel
    /// </summary>
    public class UDPVideoSource : IVideoSource
    {
        private FrameSettings frameSettings = null!;
        private ICodecSettings codecSettings = null!;
        private VideoCodec codec;

        private readonly Dictionary<UInt32, ManagedVideoFrame> frameBuffer = new();
        private readonly Dictionary<TransmissionVideoFrame, byte[]> partialFrames = new();
        private UInt32 lastFrame;
        
        private bool waitingForPPSSPS = true;
        private readonly byte[] ppssps = new byte[34];

        private bool frameWithPPSSP;
        
        private Logger logger;

        /// <inheritdoc />
        public ICodecSettings CodecSettings => codecSettings;
        
        /// <inheritdoc />
        public FrameSettings FrameSettings => frameSettings;
        
        /// <summary>
        /// ctor
        /// </summary>
        public UDPVideoSource(int listenPort)
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

            //TODO: Check if this is still needed
            //Initialise dictionary, so it can be used as a pool
            for (UInt32 i = 0; i < byte.MaxValue; i++)
            {
                lock (frameBuffer)
                {
                    frameBuffer.Add(i, new ManagedVideoFrame());
                }
            }

            if (!ServiceRegistry.TryGetService(out UDPReceiver receiver))
            {
                throw new Exception("No UDP Receiver");
            }
            
            receiver.SubscribeToReceptionTraffic(PacketType.VideoStreamPacket, FrameCallBack);
            
            return true;
        }

        private void FrameCallBack(byte[] framePacket)
        {
            FrameReception(framePacket);
            FrameCleanup();
        }
        
        private void FrameReception(byte[] receivedBytes)
        {
            if (waitingForPPSSPS)
            {
                ScanForPPS_SPS(receivedBytes);
            }
            
            TransmissionVideoFrame receivedFrame = TransmissionVideoFrame.FromUDPPacket(receivedBytes);

            if (lastFrame > receivedFrame.SequenceIDX)
                return;

            var frameData = new byte[receivedBytes.Length - TransmissionVideoFrame.HeaderSize];
            Array.Copy(receivedBytes, TransmissionVideoFrame.HeaderSize,
                frameData, 0, receivedBytes.Length - TransmissionVideoFrame.HeaderSize);

            if (receivedFrame.NumberOfPackets == 1)
            {
                lock (frameBuffer)
                {
                    frameBuffer[receivedFrame.SequenceIDX] =
                        ManagedFrameFromTransmission(receivedFrame, frameData);
                }
            }
            else
            {
                BufferPartialFrame(receivedFrame, frameData);
            }
        }

        private void ScanForPPS_SPS(byte[] receivedBytes)
        {
            byte[] marker = {0, 0, 0, 1};
            var PPS_SPSLength = 34;
            var markerPos = PPS_SPSLength + TransmissionVideoFrame.HeaderSize;
            for (int i = markerPos; i < markerPos+4; i++)
            {
                if (receivedBytes[i] != marker[i - markerPos])
                {
                    return;
                }
            }
            Array.Copy(receivedBytes, TransmissionVideoFrame.HeaderSize, ppssps, 0, PPS_SPSLength);
            waitingForPPSSPS = false;
            frameWithPPSSP = true;
        }

        private void FrameCleanup()
        {
            for (uint i = 0; i < lastFrame; i++)
            {
                lock (frameBuffer)
                {
                    if (frameBuffer.TryGetValue(i, out _))
                    {
                        frameBuffer.Remove(i);
                    }
                }
            }
        }

        private ManagedVideoFrame ManagedFrameFromTransmission(TransmissionVideoFrame receivedFrame, byte[] frameData)
        {
            if (waitingForPPSSPS)
                return new ManagedVideoFrame()
                {
                    Codec = codec,
                    Height = 0,
                    Width = 0,
                    Stream = new MemoryStream(0) 
                };
            
            lastFrame = receivedFrame.SequenceIDX;

            MemoryStream frameStream;
            if(receivedFrame.SequenceIDX == 0)
            {
                frameStream = new MemoryStream(frameData);
            }
            else
            {
                frameStream = new MemoryStream(ppssps.Length + frameData.Length);
                if (!frameWithPPSSP)
                    frameStream.Write(ppssps);
                else
                    frameWithPPSSP = false;
                frameStream.Write(frameData);
            }
            
            var ret = new ManagedVideoFrame()
            {
                Codec = codec,
                Height = receivedFrame.Height,
                Width = receivedFrame.Width,
                Stream = frameStream 
            };
            //FrameReady(ret);
            return ret;
        }
        
        //TODO: this is pretty grossly inefficient
        private void BufferPartialFrame(TransmissionVideoFrame receivedFrame, byte[] frameData)
        {
            bool GetMatchingSequencePredicate(KeyValuePair<TransmissionVideoFrame, byte[]> pf) => pf.Key.SequenceIDX == receivedFrame.SequenceIDX;

            partialFrames.Add(receivedFrame, frameData);
            
            logger.Trace($"Received ({receivedFrame.PacketIdx+1}/{receivedFrame.NumberOfPackets}) of frame {receivedFrame.SequenceIDX}");
            
            var parts = partialFrames.Where(GetMatchingSequencePredicate);
            if (parts.Count() == receivedFrame.NumberOfPackets)
            {
                AssembleFrame(receivedFrame.SequenceIDX, parts);
            }
        }

        private int assembledFrames = 0;
        private void AssembleFrame(UInt32 sequenceIDX, IEnumerable<KeyValuePair<TransmissionVideoFrame, byte[]>> parts)
        {
            var orderedParts = parts.OrderBy(pair => pair.Key.PacketIdx);
            var completedPacket = new List<byte>();
            foreach (var part in orderedParts)
            {
                completedPacket.AddRange(part.Value);
            }

            lock (frameBuffer)
            {
                frameBuffer[sequenceIDX] =
                    ManagedFrameFromTransmission(orderedParts.First().Key, completedPacket.ToArray());
            }

            foreach (var part in parts)
            {
                partialFrames.Remove(part.Key);
            }
            logger.Debug($"Assembled packets for sequence {sequenceIDX}");
            StatLogging.LogPerfStat("Assembled Frames: ", ++assembledFrames);
        }

        /// <inheritdoc />
        public IVideoFrame GetFrame()
        {
            lock (frameBuffer)
            {
                return frameBuffer[lastFrame];
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
