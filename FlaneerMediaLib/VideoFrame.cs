using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaneerMediaLib
{
    public class VideoFrame
    {
        public VideoCodec Codec;
        public int Width;
        public int Height;
        public IntPtr FrameData;
        public int FrameSize;
    }
}
