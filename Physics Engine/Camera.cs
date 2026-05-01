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
        
        private double yaw;
        private double pitch;
        readonly Config config;
        Matrix4 yawMatrix;
        public Camera()
        {
            string json = File.ReadAllText("config.json");
            config = JsonSerializer.Deserialize<Config>(json)!;
            matrix = new Matrix4();
            
        }
        
        public void move(Vec3 vector)
        {
            Vec4 v = new Vec4(vector.X, vector.Y, vector.Z, 1);
            v = yawMatrix * v;
            Globals.Camera.matrix[0, 3] += v.X;
            Globals.Camera.matrix[1, 3] += v.Y;
            Globals.Camera.matrix[2, 3] += v.Z;
            
            

            
        }
        public void rotate(int axis, double d)
        {
            if(axis == 0) pitch += d * config.sensitivity;
            if(axis == 1) yaw += d * config.sensitivity;
            pitch = Math.Clamp(pitch, -Math.PI/2, Math.PI/2);
            
            Matrix4 pitchMatrix = Matrix4.rotationMatrix(0, -pitch);
            yawMatrix = Matrix4.rotationMatrix(1, -yaw);
            Matrix4 rotation = yawMatrix * pitchMatrix;

            
            double tx = Globals.Camera.matrix[0, 3];
            double ty = Globals.Camera.matrix[1, 3];
            double tz = Globals.Camera.matrix[2, 3];

            
            Globals.Camera.matrix = rotation;

            
            Globals.Camera.matrix[0, 3] = tx;
            Globals.Camera.matrix[1, 3] = ty;
            Globals.Camera.matrix[2, 3] = tz;

        }



    }
}
