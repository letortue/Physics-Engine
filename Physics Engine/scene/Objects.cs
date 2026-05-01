using Accessibility;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Physics_Engine
{
    public struct HitResult
    {
        public bool hit;
        public Vec3 point;
        public Vec3 normal;
        public Vec3 color;
        public double t;
        public double w0;
        public double w1;
        public double w2;
        
    }
    public struct Ray
    {
        public Vec3 origin { get; set; } 
        public Vec3 direction { get; set; } 
        
        public Ray(Vec3 origin, Vec3 direction)
        {
            this.origin = origin;
            this.direction = direction;

        }
    }
    public abstract class Object
    {
        public Vec3[] coordinates { get; set; }
        public Vec3[] velocity { get; set; }
        public Vec3[] acceleration { get; set; }
        public Vec3[] color { get; set; }
        public double[] opacity { get; set; }
        public VertexAttributes attributes;
        public bool solveQuadratic(double a, double b, double c, out double? t1, out double? t2)
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
        public abstract HitResult getIntersectionPoint(Ray r);
    }




    public class Ball : Object
    {
        public double radius { get; set; }

        public Ball(Vec3 coords, VertexAttributes attributes, double radius)
        {
            this.coordinates = new Vec3[1];
            this.attributes = attributes;
            this.coordinates[0].X = coords.X;
            this.coordinates[0].Y = coords.Y;
            this.coordinates[0].Z = coords.Z;
            this.radius = radius;

        }

        public override HitResult getIntersectionPoint(Ray r)
        {

            Vec3 L = r.origin - this.coordinates[0];
            Vec3 dir = r.direction;
            HitResult result = new HitResult();
            result.color = this.attributes.colors[0];
            bool intersects = solveQuadratic(dir.dot(dir), 2 * L.dot(dir), L.dot(L) - Math.Pow(this.radius, 2), out double? t1, out double? t2);
            if (!intersects)
            {

                result.hit = false;

                return result;
            }
            else
            {
                result.hit = true;
                if (t2 == null)
                {

                    result.point = r.origin + (double)t1 * dir;
                    result.normal = (result.point - this.coordinates[0]).normalize();
                    result.t = (double) t1;
                    return result;
                }
                else
                {
                    if(t1< t2)
                    {
                        result.point = r.origin + (double)t1 * dir;
                        result.normal = (result.point - this.coordinates[0]).normalize();
                        result.t = (double)t1;
                    }
                    else
                    {
                        result.point = r.origin + (double)t2 * dir;
                        result.normal = (result.point - this.coordinates[0]).normalize();
                        result.t = (double)t2;
                    }

                    return result;
                }


            }
        }


    }

    public class Plane : Object
    {
        public Vec3 normal { get; set; }
        public Vec3 p0 { get; set; }

        public Plane( VertexAttributes attributes, Vec3 normal, Vec3 p0)
        {
            this.coordinates = new Vec3[1];
            this.coordinates[0] = p0;
            this.attributes = attributes;
            this.normal = normal;
            this.p0 = p0;
        }
        public override HitResult getIntersectionPoint(Ray r)
        {
            HitResult result = new HitResult();
            double denom = r.direction.dot(normal);
            if (Math.Abs(denom) < 1e-6) { result.hit = false; return result; }
            result.t = (p0 - r.origin).dot(normal) / denom;
            if (result.t < 0) { result.hit = false; return result; }
            result.normal = normal;
            result.hit = true;
            result.point = r.origin + (result.t * r.direction);
            result.color = this.attributes.colors[0];
            return result;
            
        }
    }
    public class Disk : Object
    {
        public Vec3 normal { get; set; }
        public double radius { get; set; }
        public double square_radius { get; set; }
        private Plane p;

        public Disk(Vec3 center, VertexAttributes attributes, Vec3 normal, double radius)
        {
            this.attributes=attributes;
            this.coordinates = new Vec3[1];
            this.normal = normal;
            this.radius = radius;
            this.square_radius = Math.Pow(radius,2);
            this.coordinates[0] = center;
            p = new Plane(attributes, normal, this.coordinates[0]);

        }
        public override HitResult getIntersectionPoint(Ray r)
        {
            

            HitResult result = p.getIntersectionPoint(r);
            double denom = r.direction.dot(normal);
            if (Math.Abs(denom) < 1e-6) { result.hit = false; return result; }

            if (!result.hit)
            {
                return result;
            }

            Vec3 v = result.point - this.coordinates[0];
            double d2 = v.dot(v);

            if (d2 <= square_radius)
            {

                result.t = (this.coordinates[0] - r.origin).dot(normal) / denom;
                if (result.t < 0) { result.hit = false; return result; }
                result.normal = normal;
                result.hit = true;
                result.point = r.origin + (result.t * r.direction);
                return result;
            }
            else
            {
                result.hit = false;
                return result;
            }







        }
    }
    public struct VertexAttributes
    {
        public Vec3[] colors;
        
        public Vec3[] velocity;
        public Vec3[] acceleration;
        double[] opacity;
    }
    public class Triangle : Object
    {
        public Vec3 normal;
        private Plane p;
        private bool onesided;
        private double area;
        
        public Triangle(Vec3[] coords, VertexAttributes attributes, bool onesided = true)
        {

            this.coordinates = new Vec3[3];
            for (int i = 0; i < coordinates.Length; i++)
            {
                coordinates[i].X = coords[i].X;
                coordinates[i].Y = coords[i].Y;
                coordinates[i].Z = coords[i].Z;
            }
            this.area = (coordinates[2] - coordinates[0]).cross(coordinates[1] - coordinates[0]).magnitude();
            this.normal = (coordinates[2] - coordinates[0]).cross(coordinates[1] - coordinates[0]).normalize();
            this.p = new(attributes, normal, coordinates[0]);
            this.onesided = onesided;
            this.attributes = attributes;
            
                

        }

        public override HitResult getIntersectionPoint(Ray r)
        {

            
            if ((onesided && normal.dot(r.direction) > 0) || Math.Abs(normal.dot(r.direction)) < 1e-6) return new HitResult { hit = false };
            
            HitResult result = p.getIntersectionPoint(r);

            if(result.t <= 0) return new HitResult { hit = false };

            Vec3 i0 = (result.point - coordinates[0]).cross(coordinates[1] - coordinates[0]);
            Vec3 i1 = (result.point - coordinates[1]).cross(coordinates[2] - coordinates[1]);
            Vec3 i2 = (result.point - coordinates[2]).cross(coordinates[0] - coordinates[2]) ;
            if (i0.dot(normal) < 0 || i1.dot(normal) < 0 || i2.dot(normal) < 0){ result.hit = false; return result;  }
            double w0 = i1.magnitude() / area;
            double w1 = i2.magnitude() / area;
            double w2 = i0.magnitude() / area;
            double R = attributes.colors[0].X * w0 + attributes.colors[1].X * w1 + attributes.colors[2].X * w2;
            double G = attributes.colors[0].Y * w0 + attributes.colors[1].Y * w1 + attributes.colors[2].Y * w2;
            double B = attributes.colors[0].Z * w0 + attributes.colors[1].Z * w1 + attributes.colors[2].Z * w2;
            result.color.X = R; result.color.Y = G; result.color.Z = B;

            //if ((Globals.timeElapsed % 100) == 0) Console.WriteLine($"{w0}, {w1}, {w2}");

            return result;
            
        }
    }

}