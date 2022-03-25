using System.Net;
using System.Net.Sockets;

namespace FlaneerMediaLib
{
    public class UDPVideoSource : IVideoSource
    {
        private FrameSettings frameSettings;
        private ICodecSettings codecSettings;
        private VideoCodec codec;
        private CyclicalFrameCounter frameCounter = new ();

        UdpClient listener;
        IPEndPoint groupEP;

        private Dictionary<int, ManagedVideoFrame> frameBuffer = new();
        private Dictionary<TransmissionVideoFrame, byte[]> partialFrames = new();
        private byte nextFrame => frameCounter.GetNext();

        private bool receiving = false;
        private bool waitingForPPS_SPS = true;
        private byte[] PPS_SPS = new byte[34];

        public Action<VideoFrame> FrameReady;

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

            if (waitingForPPS_SPS)
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
            Array.Copy(receivedBytes, TransmissionVideoFrame.HeaderSize, PPS_SPS, 0, PPS_SPSLength);
            waitingForPPS_SPS = false;
        }

        private void FrameCleanup()
        {
            //TODO: Use FrameCounter to remove old buffered packets
        }

        private ManagedVideoFrame ManagedFrameFromTransmission(TransmissionVideoFrame receivedFrame, byte[] frameData)
        {
            if (waitingForPPS_SPS)
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
                frameStream = new MemoryStream(PPS_SPS.Length + frameData.Length);
                frameStream.Write(PPS_SPS);
                frameStream.Write(frameData);
            }
            
            var ret = new ManagedVideoFrame()
            {
                Codec = codec,
                Height = receivedFrame.Height,
                Width = receivedFrame.Width,
                Stream = frameStream 
            };
            FrameReady?.Invoke(ret);
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
