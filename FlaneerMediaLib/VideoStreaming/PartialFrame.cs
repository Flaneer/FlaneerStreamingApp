using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib.VideoStreaming
{
    internal class PartialFrame
    {
        private readonly List<SmartBuffer> framePieces;

        private readonly TransmissionVideoFrame seedFrame;

        internal int ExpectedPieces => seedFrame.NumberOfPackets;
        internal int BufferedPieces => bufferedPieces;
        private int bufferedPieces;

        //TODO: refactor as eventhandler with type for args
        private readonly Action<uint, ManagedVideoFrame, bool> FrameReadyCallback;
        private SmartBufferManager smartBufferManager;

        public PartialFrame(TransmissionVideoFrame seedFrame, Action<uint, ManagedVideoFrame, bool> onFrameReady)
        {
            this.seedFrame = seedFrame;
            FrameReadyCallback = onFrameReady;
            framePieces = new List<SmartBuffer>(seedFrame.NumberOfPackets);
        
            ServiceRegistry.TryGetService(out smartBufferManager);
        }

        public void BufferPiece(SmartBuffer framePacket, int packetIdx, int packetSize)
        {
            framePieces.Insert(packetIdx, framePacket);
            bufferedPieces++;
            if (bufferedPieces == seedFrame.NumberOfPackets)
                AssembleFrame();
        }

        private void AssembleFrame()
        {
            var assembledFrame = AssembleFrameImpl(seedFrame, framePieces, smartBufferManager);
            FrameReadyCallback(seedFrame.SequenceIDX, assembledFrame, seedFrame.IsIFrame);
        }

        internal static ManagedVideoFrame AssembleFrameImpl(TransmissionVideoFrame seedFrame, List<SmartBuffer> framePieces, SmartBufferManager? smartBufferManager = null)
        {
            var frameStream = new MemoryStream(seedFrame.FrameDataSize);
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
