using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Physics_Engine
{
    public static class Maths
    {
        public static Vec3 CramersRule(Ray r, Vec3[] ABC)
        {

            Vec3 E1 = ABC[1] - ABC[0];
            Vec3 E2 = ABC[2] - ABC[0];
            Vec3 h = r.direction.Cross(E2);
            double det = E1.Dot(h);
            if (Math.Abs(det) < 1e-6) return new Vec3(-1, 0, 0); // miss
            Vec3 s = r.origin - ABC[0];
            double u = s.Dot(h) / det;
            Vec3 q = s.Cross(E1);
            double v = r.direction.Dot(q) / det;
            double t = E2.Dot(q) / det;


            return new Vec3(t, u, v);
        }
        public static bool SolveQuadratic(double a, double b, double c, out double? t1, out double? t2)
        {
            double delta = Math.Pow(b, 2) - 4 * a * c;
            if (delta < 0) { t1 = null; t2 = null; return false; }
            if (delta > 0)
            {
                double sqdelta = Math.Sqrt(delta);
                t1 = (-b - sqdelta) / (2 * a);
                t2 = (-b + sqdelta) / (2 * a);
                if (t1 > t2) { double? x = t2; t2 = t1; t1 = x; }
                return true;
            }
            t1 = -b / (2 * a);
            t2 = null;
            return true;
        }
    }
    public struct Vec3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double this[int i]
        {
            get
            {
                if (i == 0) return X;
                else if (i==1) return Y; 
                else if (i==2) return Z;
                else throw new IndexOutOfRangeException();
            }
            set
            {
                if (i == 0) X = value;
                else if (i == 1) Y = value;
                else if (i == 2) Z = value;
                else throw new IndexOutOfRangeException();
            } 
        }

        public Vec3(double X, double Y, double Z)
        {
            this.X = X; this.Y = Y; this.Z = Z;
        }

        public override string ToString()
        {
            return $"{X} {Y} {Z}";
        }

        public double Dot(Vec3 v)
        {
            return this.X * v.X + this.Y * v.Y + this.Z * v.Z;
            
        }
        public Vec3 Cross(Vec3 v)
        {
            Vec3 result = new Vec3();
            result.X = this.Y * v.Z - this.Z * v.Y;
            result.Y = this.Z * v.X - this.X * v.Z;
            result.Z = this.X * v.Y - this.Y * v.X;
            return result;
        }
        public Vec3 WorldToCamera(Matrix4 inverse)
        {

            Vec4 vf = new Vec4(this.X, this.Y, this.Z, 1);

            
            vf = inverse * vf;
            
            
            
         
            return new Vec3(vf.X, vf.Y, vf.Z);


        }

        public Vec3 Normalize()
        {
            double x = this.X;
            double y = this.Y;
            double z = this.Z;
            Vec3 v = new Vec3(x,y,z);
            double magnitude = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            if (magnitude == 0) return v;
            v.X /= magnitude;
            v.Y /= magnitude;
            v.Z /= magnitude;
            return v;
        }

        public double Magnitude()
        {
            return Math.Sqrt(Math.Pow(this.X, 2) + Math.Pow(this.Y, 2) + Math.Pow(this.Z, 2));
        }

        public static Vec3 operator +(Vec3 lhs, Vec3 rhs)
        {
            lhs.X += rhs.X; lhs.Y += rhs.Y; lhs.Z += rhs.Z;
            return lhs;
        }
        public static Vec3 operator /(Vec3 lhs, Vec3 rhs)
        {
            lhs.X /= rhs.X; lhs.Y /= rhs.Y; lhs.Z /= rhs.Z;
            return lhs;
        }
        public static Vec3 operator /(Vec3 lhs, double d)
        {
            lhs.X /= d; lhs.Y /= d; lhs.Z /= d;
            return lhs;
        }
        public static Vec3 operator -(Vec3 lhs, Vec3 rhs)
        {
            lhs.X -= rhs.X; lhs.Y -= rhs.Y; lhs.Z -= rhs.Z;
            return lhs;
        }
        public static Vec3 operator *(Vec3 lhs, Vec3 rhs)
        {
            lhs.X *= rhs.X; lhs.Y *= rhs.Y; lhs.Z *= rhs.Z;
            return lhs;
        }

        public static Vec3 operator *(double a, Vec3 rhs)
        {
            rhs.X *= a; rhs.Y *= a; rhs.Z *= a;
            return rhs;
        }

        

    }
    public struct Vec4
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double W { get; set; }
        public double this[int i]
        {
            get
            {
                if (i == 0) return X;
                else if (i == 1) return Y;
                else if (i == 2) return Z;
                else if (i == 3) return W;
                else throw new IndexOutOfRangeException();
            }
            set
            {
                if (i == 0) X = value;
                else if (i == 1) Y = value;
                else if (i == 2) Z = value;
                else if (i == 3) W = value;
                else throw new IndexOutOfRangeException();
            }
        }
        public Vec4(double X, double Y, double Z, double W)
        {
            this.X = X; this.Y = Y; this.Z = Z; this.W = W;
        }

        public override string ToString()
        {
            return $"{X} {Y} {Z} {W}";
        }

        public Vec4 Dot(Vec4 v)
        {
            return new Vec4(this.X * v.X, this.Y * v.Y, this.Z * v.Z, this.W * v.W);

        }
        public void Normalize()
        {
            double magnitude = 1 / Math.Sqrt(Math.Pow(this.X, 2) + Math.Pow(this.Z, 2) + Math.Pow(this.Z, 2));
            this.X *= magnitude;
            this.Y *= magnitude;
            this.Z *= magnitude;
        }
        

        public static Vec4 operator +(Vec4 lhs, Vec4 rhs)
        {
            return new Vec4(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z, lhs.W + rhs.W);
        }
        public static Vec4 operator -(Vec4 lhs, Vec4 rhs)
        {
            return new Vec4(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z, lhs.W - rhs.W);
        }

        public static Vec4 operator *(double a, Vec4 rhs)
        {
            return new Vec4(a * rhs.X, a * rhs.Y, a * rhs.Z, a * rhs.W);
        }



    }
    public struct Matrix3
    {
        public double[,] data;
        private static Config config { get; set; }
        public override string ToString()
        {
            return $"{data[0, 0]} {data[0, 1]} {data[0, 2]}  {Environment.NewLine} {data[1, 0]} {data[1, 1]} {data[1, 2]} {Environment.NewLine} {data[2, 0]} {data[2, 1]} {data[2, 2]}   ";
        }

        public double this[int i, int j]
        {
            get => data[i, j];
            set => data[i, j] = value;
        }
        public Vec3 this[int i]
        {
            get
            {
                return new Vec3(data[0,i], data[1,i], data[2,i]);
            }
            set
            {
                data[0, i] = value.X;
                data[1, i] = value.Y;
                data[2, i] = value.Z;
            }

        }

        public Matrix3(bool zeros = false)
        {
            string json = File.ReadAllText("config.json");

            config = JsonSerializer.Deserialize<Config>(json)!;
            if (zeros == true)
            {
                this.data = new double[3, 3]
                {
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 0, 0, 0 }
                
            };
            }
            else
            {
                this.data = new double[3, 3] {
                { 1, 0, 0},
                { 0, 1, 0},
                { 0, 0, 1 }
                
            };
            }


        }
        public double Determinant()
        {
            Matrix3 m = this;
            return (m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1]))
                - (m[1, 0] * (m[0, 1] * m[2, 2] - m[2, 1] * m[0, 2]))
                + (m[2, 0] * (m[0, 1] * m[1, 2] - m[1, 1] * m[0, 2]));
        }
        
        public Matrix3 Transpose3()
        {
            Matrix3 m = new Matrix3();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    m[i, j] = this.data[j, i];
                }
            }
            return m;
        }


        public static Matrix3 RotationMatrix(int axis, double degrees)
        {
            Matrix3 m = new Matrix3();
            if (axis == 0)
            {
                m[1, 1] = Math.Cos(degrees);
                m[2, 2] = Math.Cos(degrees);
                m[1, 2] = -Math.Sin(degrees);
                m[2, 1] = Math.Sin(degrees);
            }
            else
            if (axis == 1)
            {
                m[0, 0] = Math.Cos(degrees);
                m[0, 2] = Math.Sin(degrees);
                m[2, 0] = -Math.Sin(degrees);
                m[2, 2] = Math.Cos(degrees);
            }
            else
            if (axis == 2)
            {
                m[0, 0] = Math.Cos(degrees);
                m[0, 1] = -Math.Sin(degrees);
                m[1, 0] = Math.Sin(degrees);
                m[1, 1] = Math.Cos(degrees);
            }
            return m;
        }
        


        public static Matrix3 operator *(Matrix3 m1, Matrix3 m2)
        {
            Matrix3 m = new Matrix3();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    m[i, j] = (m1[i, 0] * m2[0, j]) + (m1[i, 1] * m2[1, j]) + (m1[i, 2] * m2[2, j]);
                }
            }
            return m;
        }
        public static Matrix3 operator +(Matrix3 m1, Matrix3 m2)
        {
            Matrix3 m = new Matrix3();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    m[i, j] = m1[i, j] + m2[i, j];
                }
            }
            return m;
        }


        public static Vec3 operator *(Matrix3 m, Vec3 v)
        {
            Vec3 v1 = new Vec3(0, 0, 0);
            v1.X = (m[0, 0] * v.X) + (m[0, 1] * v.Y) + (m[0, 2] * v.Z);
            v1.Y = (m[1, 0] * v.X) + (m[1, 1] * v.Y) + (m[1, 2] * v.Z);
            v1.Z = (m[2, 0] * v.X) + (m[2, 1] * v.Y) + (m[2, 2] * v.Z);


            return v1;
        }
    }
    public class Matrix4
    {
        public double[,] data;
        private static Config config { get; set; }
        public override string ToString()
        {
            return $"{data[0,0]} {data[0, 1]} {data[0, 2]} {data[0, 3]} {Environment.NewLine} {data[1, 0]} {data[1, 1]} {data[1, 2]} {data[1, 3]} {Environment.NewLine} {data[2, 0]} {data[2, 1]} {data[2, 2]} {data[2, 3]} {Environment.NewLine} {data[3, 0]} {data[3, 1]} {data[3, 2]} {data[3, 3]}";
        }

        public double this[int i, int j]
        {
            get => data[i, j];
            set => data[i, j] = value;
        }
        

        public Matrix4(bool zeros = false)
        {
            string json = File.ReadAllText("config.json");

            config = JsonSerializer.Deserialize<Config>(json)!;
            if (zeros == true)
            {
                this.data = new double[4,4]
                {
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 }
            };
            }
            else
            {
                this.data = new double[4,4] {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            };
            }
            
        
        }
        public Matrix4 Transpose4()
        {
            Matrix4 m = new();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0;  j < 4;  j++)
                {
                    m[i,j] = data[j,i];
                }
            }
            return m;
        }
        public Matrix4 Transpose3()
        {
            Matrix4 m = new Matrix4();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0;  j < 3;  j++)
                {
                    m[i,j] = this.data[j,i];
                }
            }
            return m;
        }
        public static Matrix4 ProjectionMatrix()
        {
            Matrix4 m = new Matrix4();
            m[3, 2] = -1;
            m[3, 3] = 0;
            m[2, 2] = -config.Clipping_range / (config.Clipping_range - 1); 
            m[2, 3] = -config.Clipping_range / (config.Clipping_range - 1);
            double f = 1 / Math.Tan(config.FOV / 2 / 57.2958D);
            m[0, 0] = f;
            m[1, 1] = f;
            return m;
        }
        public static Matrix4 RotationMatrix(int axis, double degrees)
        {
            Matrix4 m = new Matrix4();
            if(axis == 0)
            {
                m[1,1] = Math.Cos(degrees);
                m[2,2] = Math.Cos(degrees);
                m[1,2] = -Math.Sin(degrees);
                m[2,1] = Math.Sin(degrees);
            }
            else
            if(axis == 1)
            {
                m[0,0] = Math.Cos(degrees);
                m[0,2] = Math.Sin(degrees);
                m[2,0] = -Math.Sin(degrees);
                m[2,2] = Math.Cos(degrees);
            }
            else
            if (axis == 2)
            {
                m[0, 0] = Math.Cos(degrees);
                m[0, 1] = -Math.Sin(degrees);
                m[1, 0] = Math.Sin(degrees);
                m[1, 1] = Math.Cos(degrees);
            }
                return m;
        }
        public Matrix4 InverseRotationPart()
        {
            Matrix4 R_T = this.Transpose3();
            
            Vec3 t = new();
            t.X = this[0, 3];
            t.Y = this[1, 3];
            t.Z = this[2, 3];
            Vec3 u = new();
            u.X = -(R_T[0,0] * t.X + R_T[0, 1] * t.Y + R_T[0, 2] * t.Z);
            u.Y = -(R_T[1, 0] * t.X + R_T[1, 1] * t.Y + R_T[1, 2] * t.Z);
            u.Z = -(R_T[2, 0] * t.X + R_T[2, 1] * t.Y + R_T[2, 2] * t.Z);
            Matrix4 final = new Matrix4(true);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    final[i, j] = R_T[i, j];
                }
            }
            final.data[0,3] = u.X; final.data[1, 3] = u.Y; final.data[2, 3] = u.Z;  
            final.data[3, 3] = 1;
            
            return final;

        }

        
        public static Matrix4 operator *(Matrix4 m1, Matrix4 m2)
        {
            Matrix4 m = new Matrix4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    m[i, j] = (m1[i, 0] * m2[0, j]) + (m1[i, 1] * m2[1, j]) + (m1[i, 2] * m2[2, j]) + (m1[i, 3] * m2[3, j]);
                }
            }
            return m;
        }
        public static Matrix4 operator +(Matrix4 m1, Matrix4 m2)
        {
            Matrix4 m = new Matrix4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    m[i,j] = m1[i,j] + m2[i,j];
                }
            }
            return m;
        }

        public static Vec4 MovementMult(Matrix4 m, Vec4 v)
        {
            Vec4 v1 = new Vec4(0, 0, 0, 1);

            
            v1.X = ((m[0, 0] * v.X) + (m[0, 1] * v.Y) + (m[0, 2] * v.Z) + (m[0, 3] * v.W));
            v1.Y = ((m[1, 0] * v.X) + (m[1, 1] * v.Y) + (m[1, 2] * v.Z) + (m[1, 3] * v.W));
            v1.Z = (m[2, 0] * v.X) + (m[2, 1] * v.Y) + (m[2, 2] * v.Z) + (m[2, 3] * v.W);
                
            
            return v1;
        }
        public static Vec4 operator *(Matrix4 m, Vec4 v)
        {
            Vec4 v1 = new Vec4(0, 0, 0, 1);
            double W;
            if (v.W != 0) W = ((m[3, 0] * v.X) + (m[3, 1] * v.Y) + (m[3, 2] * v.Z) + (m[3, 3] * v.W));
            else W = 1;

            v1.X = ((m[0, 0] * v.X) + (m[0, 1] * v.Y) + (m[0, 2] * v.Z) + (m[0, 3] * v.W)) / W;
            v1.Y = ((m[1, 0] * v.X) + (m[1, 1] * v.Y) + (m[1, 2] * v.Z) + (m[1, 3] * v.W)) / W;
            v1.Z = ((m[2, 0] * v.X) + (m[2, 1] * v.Y) + (m[2, 2] * v.Z) + (m[2, 3] * v.W)) / W;


            return v1;
        }
    }
}
