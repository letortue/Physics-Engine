using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Engine
{
    public class Save
    {
        public string filepath;
        public int[] resolution;
        public string name;

        public Save(string filepath, int[] resolution, string name)
        {
            this.filepath = filepath;
            this.resolution = resolution;
            this.name = name;
        }
    }
}
