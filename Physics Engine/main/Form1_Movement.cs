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
            if (config.PreLoad) return;

            int dx = e.X - (this.Width / 2);
            int dy = e.Y - (this.Height / 2);

            camera.Rotate(0, dy);
            camera.Rotate(1, dx);

            Point center = new Point(this.Width / 2, this.Height / 2);
            Cursor.Position = PointToScreen(center);
        }
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (config.PreLoad) return;
            switch (e.KeyCode)
            {
                case Keys.W:
                    keysPressed["W"] = true;
                    return;
                case Keys.A:
                    keysPressed["A"] = true;
                    return;
                case Keys.S:
                    keysPressed["S"] = true;
                    return;
                case Keys.D:
                    keysPressed["D"] = true;
                    return;
                case Keys.ControlKey:
                    keysPressed["CTRL"] = true;
                    return;
                case Keys.Space:
                    keysPressed["SPACE"] = true;
                    return;
            }
            

        }
        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (config.PreLoad) return;
            switch (e.KeyCode)
            { 
                case Keys.W:
                    keysPressed["W"] = false;
                    return;
                case Keys.A:
                    keysPressed["A"] = false;
                    return;
                case Keys.S:
                    keysPressed["S"] = false;
                    return;
                case Keys.D:
                    keysPressed["D"] = false;
                    return;
                case Keys.ControlKey:
                    keysPressed["CTRL"] = false;
                    return;
                case Keys.Space:
                    keysPressed["SPACE"] = false;
                    return;
            }

            
        }
    }
}
