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
        private Dictionary<TransmissionVideoFrame, byte[]> partialFrames = new();
        private byte nextFrame = 0;

        private bool receiving = false;

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
                    byte[] receivedBytes = listener.Receive(ref groupEP);
                    var parsedBroadcast = TransmissionVideoFrame.FromUDPPacket(receivedBytes);
                    TransmissionVideoFrame receivedFrame = parsedBroadcast.Item1;
                    if (receivedFrame.NumberOfPackets == 1)
                    {
                        lock (frameBuffer)
                        {
                            frameBuffer[receivedFrame.SequenceIDX] = 
                                ManagedFrameFromTransmission(receivedFrame, parsedBroadcast.Item2);
                        }
                    }
                    else
                    {
                        BufferPartialFrame(receivedFrame, parsedBroadcast.Item2);
                    }
                }
            });
        }

        private ManagedVideoFrame ManagedFrameFromTransmission(TransmissionVideoFrame receivedFrame, byte[] frameData)
        {
            return new ManagedVideoFrame()
            {
                Codec = codec,
                Height = receivedFrame.Height,
                Width = receivedFrame.Width,
                Stream = new MemoryStream(frameData)
            };
        }
        
        //TODO: this is pretty grossly inefficient
        private void BufferPartialFrame(TransmissionVideoFrame receivedFrame, byte[] frameData)
        {
            bool GetMatchingSequencePredicate(KeyValuePair<TransmissionVideoFrame, byte[]> pf) => pf.Key.SequenceIDX == receivedFrame.SequenceIDX;

            partialFrames.Add(receivedFrame, frameData);
            
            var parts = partialFrames.Where(GetMatchingSequencePredicate);
            if (parts.Count() == receivedFrame.NumberOfPackets)
            {
                var orderedParts = parts.OrderBy(pair => pair.Key.PacketIdx);
                var completedPacket = new List<byte>();
                foreach (var part in orderedParts)
                {
                    completedPacket.AddRange(part.Value);
                }

                lock (frameBuffer)
                {
                    frameBuffer[receivedFrame.SequenceIDX] =
                        ManagedFrameFromTransmission(orderedParts.First().Key, completedPacket.ToArray());
                }
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
                frameBuffer.Remove(nextFrame);
                IncrementNextFrame();
                return ret;
            }
        }

        private void IncrementNextFrame()
        {
            if (nextFrame + 1 > byte.MaxValue)
            {
                nextFrame = 0;
            }
            else
            {
                nextFrame++;
            }
        }
        
        public void Dispose()
        {
            receiving = false;
            listener.Dispose();
        }
    }
}
