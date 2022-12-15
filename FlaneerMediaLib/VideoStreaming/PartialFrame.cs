using FlaneerMediaLib.SmartStorage;
using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib.VideoStreaming
{
    internal class PartialFrame
    {
        private Dictionary<int, SmartBuffer> framePieces;
        
        private readonly TransmissionVideoFrame seedFrame;

        internal int ExpectedPieces => seedFrame.NumberOfPackets;
        internal int BufferedPieces => bufferedPieces;
        private int bufferedPieces;

        //TODO: refactor as eventhandler with type for args
        private readonly Action<uint, UnassembledFrame, bool> FrameReadyCallback;
        private SmartBufferManager smartBufferManager;
        private SmartMemoryStreamManager smartMemoryStreamManager;

        //This array contains the indices of the framePieces, e.g. if orderedIndices[0] is 3 that means the first part is at framePieces[3]
        private readonly int[] orderedIndices;

        public PartialFrame(TransmissionVideoFrame seedFrame, Action<uint, UnassembledFrame, bool> onFrameReady)
        {
            this.seedFrame = seedFrame;
            FrameReadyCallback = onFrameReady;
            framePieces = new Dictionary<int, SmartBuffer>(seedFrame.NumberOfPackets);

            orderedIndices = new int[seedFrame.NumberOfPackets];
            
            ServiceRegistry.TryGetService(out smartBufferManager);
            ServiceRegistry.TryGetService(out smartMemoryStreamManager);
        }

        public void BufferPiece(SmartBuffer framePacket, int packetIdx, int packetSize)
        {
            orderedIndices[packetIdx] = framePieces.Count;
            framePieces.Add(packetIdx, framePacket);
            bufferedPieces++;
            if (bufferedPieces == seedFrame.NumberOfPackets)
                AssembleFrame();
        }

        private void AssembleFrame()
        {
            //TODO: MAKE CODEC ACCESSIBLE THROUGH SERVICE MANAGER
            var unassembledFrame = new PartialUnassembledFrame(VideoCodec.H264, seedFrame, framePieces, orderedIndices);
            FrameReadyCallback(seedFrame.SequenceIDX, unassembledFrame, seedFrame.IsIFrame);
        }
    }
}
