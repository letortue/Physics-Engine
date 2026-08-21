using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Engine.scene
{
    public class SaveConfig
    {
        public bool PreLoad { get; set; }
        public int FrameAmount { get; set; }
        public bool IsWrite { get; set; }
        public bool IsRead { get; set; }
        public bool IsPlayback { get; set; }
        public bool IsRepeat { get; set; }

        public SaveConfig(bool PreLoad, int FrameAmount, bool IsWrite, bool IsRead, bool IsPlayback, bool IsRepeat) 
        {
            this.PreLoad = PreLoad;
            this.FrameAmount = FrameAmount;
            this.IsWrite = IsWrite;
            this.IsRead = IsRead;
            this.IsPlayback = IsPlayback;
            this.IsRepeat = IsRepeat;


        }
        public SaveConfig() 
        {
            this.PreLoad=false;
            this.FrameAmount=1;
            this.IsWrite=false;
            this.IsRead=false;
            this.IsPlayback=true;
            this.IsRepeat=false;
        }
    }
}
