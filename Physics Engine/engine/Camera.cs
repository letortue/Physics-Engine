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
        readonly Config_Engine config;
        Matrix4 yawMatrix;
        public Camera(Config_Engine config, Matrix4 matrix = null)
        {
            this.config = config;
            /*
            string json = File.ReadAllText("config.json");
            config = JsonSerializer.Deserialize<Config_Engine>(json)!;
            */
            this.matrix = matrix == null ? new Matrix4() : matrix;
            
        }
        public static Matrix4 CreateCameraMatrix(double[,] data)
        {
            Matrix4 m = new Matrix4(false);
            m.data = data;
            return m;
        }
        public void Move(Vec3 vector)
        {
            Vec4 v = new Vec4(vector.X, vector.Y, vector.Z, 1);
            v = yawMatrix * v;
            this.matrix[0, 3] += v.X;
            this.matrix[1, 3] += v.Y;
            this.matrix[2, 3] += v.Z;
            
            

            
        }
        public void Rotate(int axis, double d)
        {
            if(axis == 0) pitch += d * config.Sensitivity;
            if(axis == 1) yaw += d * config.Sensitivity;
            pitch = Math.Clamp(pitch, -Math.PI/2, Math.PI/2);
            
            Matrix4 pitchMatrix = Matrix4.RotationMatrix(0, -pitch);
            yawMatrix = Matrix4.RotationMatrix(1, -yaw);
            Matrix4 rotation = yawMatrix * pitchMatrix;

            
            double tx = this.matrix[0, 3];
            double ty = this.matrix[1, 3];
            double tz = this.matrix[2, 3];

            
            this.matrix = rotation;

            
            this.matrix[0, 3] = tx;
            this.matrix[1, 3] = ty;
            this.matrix[2, 3] = tz;

        }



    }
}
