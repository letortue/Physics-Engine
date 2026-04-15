using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            matrix[0, 3] += vector.X;
            matrix[1, 3] += vector.Y;
            matrix[2, 3] += vector.Z;
            //Console.WriteLine(matrix);
        }
        public void rotate(int axis, double degrees)
        {
            Matrix4 m =  new Matrix4();
            matrix = matrix * m.rotationMatrix(axis, degrees);
        }

        
    }
}
