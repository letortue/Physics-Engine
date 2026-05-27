using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
namespace Physics_Engine
{
    internal class Config
    {
        public int[] Image_res {get; set;}
        public int[] Canvas_res {get; set;}
        public int Interval { get; set; }
        public double Sensitivity { get; set; }
        public double Clipping_range { get; set; }
        public double FOV { get; set; }
        public double Movement_speed { get; set; }
        public double[] BackgroundColor { get; set; }
        public int AntiAliasingRes { get; set; }



    }
}
