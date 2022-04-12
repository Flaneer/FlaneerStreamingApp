using System.Net;
using System.Net.Sockets;
using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib
{
    public class UDPVideoSource : IVideoSource
    {
        private FrameSettings frameSettings = null!;
        private ICodecSettings codecSettings = null!;
        private VideoCodec codec;
        private readonly CyclicalFrameCounter frameCounter = new ();

        private readonly UdpClient listener;
        private IPEndPoint groupEP;

        private readonly Dictionary<int, ManagedVideoFrame> frameBuffer = new();
        private readonly Dictionary<TransmissionVideoFrame, byte[]> partialFrames = new();
        private byte nextFrame => frameCounter.GetNext();

        private bool receiving;
        private bool waitingForPPSSPS = true;
        private readonly byte[] ppssps = new byte[34];

        public Action<VideoFrame> FrameReady = null!;

        public UDPVideoSource(int listenPort)
        {
            listener = new UdpClient(listenPort);
            groupEP = new IPEndPoint(IPAddress.Any, listenPort);
        }

        public ICodecSettings CodecSettings => codecSettings;

        public FrameSettings FrameSettings => frameSettings;

        public bool Init(FrameSettings frameSettingsIn, ICodecSettings codecSettingsIn)
        {
            this.frameSettings = frameSettingsIn;
            this.codecSettings = codecSettingsIn;
            switch (codecSettingsIn)
            {
                case H264CodecSettings:
                    codec = VideoCodec.H264;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(codecSettingsIn));
            }

            //Initialise dictionary, so it can be used as a pool
            for (int i = 0; i < byte.MaxValue; i++)
            {
                lock (frameBuffer)
                {
                    frameBuffer.Add(i, new ManagedVideoFrame());
                }
            }

            receiving = true;
            BeginReceptionThread();
            
            return true;
        }

        private void BeginReceptionThread()
        {
            Task.Run(() =>
            {
                while (receiving)
                {
                    FrameReception();
                    FrameCleanup();
                }
            });
        }

        private void FrameReception()
        {
            byte[] receivedBytes;
            try
            {
                receivedBytes = listener.Receive(ref groupEP);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            if (waitingForPPSSPS)
            {
                ScanForPPS_SPS(receivedBytes);
            }
            
            TransmissionVideoFrame receivedFrame = TransmissionVideoFrame.FromUDPPacket(receivedBytes);

            if (frameCounter.IsOlder(receivedFrame.SequenceIDX))
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
        }

        private void FrameCleanup()
        {
            //TODO: Use FrameCounter to remove old buffered packets
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
            
            frameCounter.SkipTo(receivedFrame.SequenceIDX);

            MemoryStream frameStream;
            if(receivedFrame.SequenceIDX == 0)
            {
                frameStream = new MemoryStream(frameData);
            }
            else
            {
                frameStream = new MemoryStream(ppssps.Length + frameData.Length);
                frameStream.Write(ppssps);
                frameStream.Write(frameData);
            }
            
            var ret = new ManagedVideoFrame()
            {
                Codec = codec,
                Height = receivedFrame.Height,
                Width = receivedFrame.Width,
                Stream = frameStream 
            };
            FrameReady(ret);
            return ret;
        }
        
        //TODO: this is pretty grossly inefficient
        private void BufferPartialFrame(TransmissionVideoFrame receivedFrame, byte[] frameData)
        {
            bool GetMatchingSequencePredicate(KeyValuePair<TransmissionVideoFrame, byte[]> pf) => pf.Key.SequenceIDX == receivedFrame.SequenceIDX;

            partialFrames.Add(receivedFrame, frameData);
            
            var parts = partialFrames.Where(GetMatchingSequencePredicate);
            if (parts.Count() == receivedFrame.NumberOfPackets)
            {
                AssembleFrame(receivedFrame.SequenceIDX, parts);
            }
        }

        private void AssembleFrame(int sequenceIDX, IEnumerable<KeyValuePair<TransmissionVideoFrame, byte[]>> parts)
        {
            TransmissionVideoFrame receivedFrame;
            var orderedParts = parts.OrderBy(pair => pair.Key.PacketIdx);
            var completedPacket = new List<byte>();
            foreach (var part in orderedParts)
            {
                completedPacket.AddRange(part.Value);
            }

            lock (frameBuffer)
            {
                Console.WriteLine($"Assembling packets for sequence {sequenceIDX}");
                frameBuffer[sequenceIDX] =
                    ManagedFrameFromTransmission(orderedParts.First().Key, completedPacket.ToArray());
            }

            foreach (var part in parts)
            {
                partialFrames.Remove(part.Key);
            }
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
            lock (frameBuffer)
            {
                var ret = frameBuffer[nextFrame];
                if (ret.Stream.Length != 0)
                    frameCounter.Increment();
                return ret;
            }
        }

        public void Dispose()
        {
            receiving = false;
            listener.Dispose();
        }
    }
}
