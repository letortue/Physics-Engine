using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Engine
{
    public class Config_Render
    {
        public double Clipping_range { get; set; }
        public double FOV { get; set; }
        public double Movement_speed { get; set; }
        public double[] BackgroundColor { get; set; }
        public int AntiAliasingRes { get; set; }
        public int LightDampener { get; set; }
    }
}
