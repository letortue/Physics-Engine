using Aspose.ThreeD.Formats;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Engine
{
    public class Transform
    {
        

        private double yaw;
        private double pitch;
        public Vec3 position;
        private double scaleFactor;
        

        public Transform()
        {
            
            this.yaw = 0;
            this.pitch = 0;
            this.position = new Vec3();
            this.scaleFactor = 1;
            
        }
        public Transform(double yaw, double pitch, double factor, Vec3 displacement)
        {
            
            this.yaw = yaw;
            this.pitch = pitch;
            this.position = displacement;
            this.scaleFactor = factor;

        }
        public Matrix4 GetModelMatrix()
        {
            
            Matrix4 scaleMat = Matrix4.CreateScale(scaleFactor);

            Matrix4 pitchMat = Matrix4.RotationMatrix(0, pitch);
            Matrix4 yawMat = Matrix4.RotationMatrix(1, yaw);
            Matrix4 rotMat = yawMat * pitchMat;

            Matrix4 transMat = Matrix4.CreateTranslation(position);

            
            return transMat * rotMat * scaleMat;
        }
        /*
        public void Move(Vec3 vector)
        {
            position += vector;


        }
        public void Rotate(int axis, double radians)
        {
            if (axis == 0) pitch += radians;
            if (axis == 1) yaw += radians;
            pitch = Math.Clamp(pitch, -Math.PI / 2, Math.PI / 2);

        }
        public void Scale(double factor)
        {
            scaleFactor = factor;
        }
        */
    }
}
