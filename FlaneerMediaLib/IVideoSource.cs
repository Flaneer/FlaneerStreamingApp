using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaneerMediaLib
{
    public interface IVideoSource : IService, IDisposable
    {
        public void Init(FrameSettings frameSettings, ICodecSettings codecSettings);
    }
}
