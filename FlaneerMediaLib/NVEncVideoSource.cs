using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaneerMediaLib
{
    internal class NvEncVideoSource : IVideoSource, IEncoder
    {
        public void Init(FrameSettings frameSettings, ICodecSettings codecSettings)
        {
        }

        public VideoFrame GetFrame()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
