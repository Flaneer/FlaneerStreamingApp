using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

using FF = FFmpeg.AutoGen.ffmpeg;

namespace FlaneerMediaLib.VideoStreaming.ffmpeg
{
    /// <summary>
    /// Audio video stream reader
    /// </summary>
    public class AVIOReader : IDisposable
    {
        /// <summary>
        /// AVIO context pointer, used for ffmpeg
        /// </summary>
        public unsafe AVIOContext* AvioCtx => avioCtx;

        private MemoryStream inputStream;
        private int bufferSize;
        private unsafe byte * bufferPtr;
        private unsafe AVIOContext * avioCtx;
        private readonly avio_alloc_context_read_packet readDel;
        private readonly avio_alloc_context_seek seekDel;
        private byte[] imageBytes;

        /// <summary>
        /// ctor
        /// </summary>
        public unsafe AVIOReader(MemoryStream inputStream)
        {
            this.inputStream = inputStream;
            bufferSize = (int) inputStream.Length;
            readDel = Read;
            seekDel = Seek;

            AllocAvioContext();
            
            imageBytes = new byte[bufferSize];
        }

        private unsafe void AllocAvioContext()
        {
            fixed (byte* p = inputStream.GetBuffer())
            {
                bufferPtr = p;
                //Allocate and initialize an AVIOContext for buffered I/O. It must be later freed with avio_context_free().
                avioCtx = FF.avio_alloc_context(bufferPtr, (int) inputStream.Length, 0, null, readDel, null, seekDel);
            }
        }

        /// <summary>
        /// Refresh the stream containing the video input
        /// </summary>
        public void RefreshInputStream(MemoryStream streamIn)
        {
            inputStream = streamIn;
            bufferSize = (int) inputStream.Length;
            AllocAvioContext();
        }

        private unsafe int Read(void* opaque, byte *buf, int bufSize)
        {
            inputStream.Position = 0;
            if (inputStream.Length > imageBytes.Length)
                imageBytes = new byte[inputStream.Length];
            var bytesRead = inputStream.Read(imageBytes, 0, (int)inputStream.Length);
            Marshal.Copy(imageBytes, 0, (IntPtr)buf, (int)inputStream.Length);
            //https://ffmpeg.org/doxygen/trunk/avio_8h.html#a853f5149136a27ffba3207d8520172a5
            //"must never return 0 but rather a proper AVERROR code."
            return bytesRead == 0 ? (int)inputStream.Length : bytesRead;
        }

        private unsafe Int64 Seek(void* opaque, Int64 offset, int whence) 
        {
            if (0x10000 == whence)
                return inputStream.Length;

            return inputStream.Seek(offset, (SeekOrigin)whence); 
        }

        /// <inheritdoc />
        public unsafe void Dispose()
        {
            inputStream.Dispose();
            FF.av_free(avioCtx);
            FF.av_free(bufferPtr);
        }
    }
}
