using Aspose.ThreeD;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Engine.Engine
{
    public interface IRenderEngine
    {

        byte[] RenderImage(Scene scene);
        

    }
}
