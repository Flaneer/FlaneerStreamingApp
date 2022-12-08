using FlaneerMediaLib.SmartStorage;
using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib.VideoStreaming
{
    internal class PartialFrame
    {
        private Dictionary<int, SmartBuffer> framePieces;

        private List<SmartBuffer> OrderedFramePieces => framePieces.OrderBy(x => x.Key).Select(x => x.Value).ToList();

        private readonly TransmissionVideoFrame seedFrame;

        internal int ExpectedPieces => seedFrame.NumberOfPackets;
        internal int BufferedPieces => bufferedPieces;
        private int bufferedPieces;

        //TODO: refactor as eventhandler with type for args
        private readonly Action<uint, ManagedVideoFrame, bool> FrameReadyCallback;
        private SmartBufferManager smartBufferManager;
        private SmartMemoryStreamManager smartMemoryStreamManager;

        public PartialFrame(TransmissionVideoFrame seedFrame, Action<uint, ManagedVideoFrame, bool> onFrameReady)
        {
            this.seedFrame = seedFrame;
            FrameReadyCallback = onFrameReady;
            framePieces = new Dictionary<int, SmartBuffer>(seedFrame.NumberOfPackets);
        
            ServiceRegistry.TryGetService(out smartBufferManager);
            ServiceRegistry.TryGetService(out smartMemoryStreamManager);
        }

        public void BufferPiece(SmartBuffer framePacket, int packetIdx, int packetSize)
        {
            framePieces.Add(packetIdx, framePacket);
            bufferedPieces++;
            if (bufferedPieces == seedFrame.NumberOfPackets)
                AssembleFrame();
        }

        private void AssembleFrame()
        {
            var assembledFrame = AssembleFrameImpl(smartMemoryStreamManager, seedFrame, OrderedFramePieces, smartBufferManager);
            FrameReadyCallback(seedFrame.SequenceIDX, assembledFrame, seedFrame.IsIFrame);
        }

        internal static ManagedVideoFrame AssembleFrameImpl(SmartMemoryStreamManager smartMemoryStreamManager, TransmissionVideoFrame seedFrame, List<SmartBuffer> framePieces, SmartBufferManager? smartBufferManager = null)
        {
            var frameStream = smartMemoryStreamManager.GetStream(seedFrame.FrameDataSize);
            
            foreach (var framePiece in framePieces)
            {
                frameStream.Write(framePiece.Buffer);
                smartBufferManager?.ReleaseBuffer(framePiece);
            }

            var assembledFrame = new ManagedVideoFrame()
            {
                Codec = seedFrame.Codec,
                Height = seedFrame.Height,
                Width = seedFrame.Width,
                Stream = frameStream
            };
            return assembledFrame;
        }
    }
}
