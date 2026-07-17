using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
namespace Physics_Engine
{
    public class Config_Engine
    {
        public int[] Image_res {get; set;}
        public int[] Canvas_res {get; set;}
        public int Interval { get; set; }
        public double Sensitivity { get; set; }
        public bool PreLoad { get; set; }
        public int FrameAmount { get; set; }
        public string ReadRenderPath { get; set; }
        public string WriteRenderPath { get; set; }
        public bool IsWrite { get; set; }
        public bool IsRead { get; set; }
        public bool IsPlayback { get; set; }
        public bool IsRepeat { get; set; }
        



    }
}
