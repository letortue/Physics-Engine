using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace Physics_Engine
{
    public class Camera
    {

        public Matrix4 matrix;
        public Vec3 worldPos;


        public Camera()
        {
            
            matrix = new Matrix4();
            worldPos = new Vec3(matrix[0,3], matrix[1, 3], matrix[2, 3]);
        }
        
        public void move(Vec3 vector)
        {
            Vec4 v = new Vec4(vector.X, vector.Y, vector.Z, 0);
            v = Globals.Camera.matrix * v;
            Globals.Camera.matrix[0, 3] += v.X;
            Globals.Camera.matrix[1, 3] += v.Y;
            Globals.Camera.matrix[2, 3] += v.Z;
            
            

            
        }
        public void rotate(int axis, double degrees)
        {
            Matrix4 m =  new Matrix4();
            matrix = matrix * m.rotationMatrix(axis, degrees);


        }



    }
}
