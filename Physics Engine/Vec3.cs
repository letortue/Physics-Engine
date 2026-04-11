using OpenTK.Graphics.OpenGL;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Engine
{
    public struct Vec3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vec3(double X, double Y, double Z)
        {
            this.X = X; this.Y = Y; this.Z = Z;
        }

        public override string ToString()
        {
            return $"{X} {Y} {Z}";
        }

        public Vec3 dot(Vec3 v)
        {
            return new Vec3(this.X * v.X, this.Y * v.Y, this.Z * v.Z);
            
        }

        public static Vec3 operator +(Vec3 lhs, Vec3 rhs)
        {
            return new Vec3(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        }
        public static Vec3 operator -(Vec3 lhs, Vec3 rhs)
        {
            return new Vec3(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
        }

        public static Vec3 operator *(double a, Vec3 rhs)
        {
            return new Vec3(a * rhs.X, a * rhs.Y, a * rhs.Z);
        }

        

    }
}
