using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Engine
{
    public partial class Form1
    {
        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("2");
            Point? center = engine.MouseMove(e);
            if(center.HasValue) Cursor.Position = PointToScreen((Point) center);
        }
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("3");
            if (engine.KeyDown(e)) return;

        }
        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (engine.KeyUp(e)) return;
        }
    
    }
}
