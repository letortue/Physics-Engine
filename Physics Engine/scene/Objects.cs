using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using System;
using System.Collections.Generic;
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
        public double t;
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

        public Ball(Vec3 coords, Vec3 v, Vec3 a, double radius)
        {
            this.coordinates = new Vec3[1];
            this.velocity = new Vec3[1];
            this.acceleration = new Vec3[1];

            this.coordinates[0].X = coords.X;
            this.velocity[0].X = v.X;
            this.acceleration[0].X = a.X;
            this.coordinates[0].Y = coords.Y;
            this.velocity[0].Y = v.Y;
            this.acceleration[0].Y = a.Y;
            this.coordinates[0].Z = coords.Z;
            this.velocity[0].Z = v.Z;
            this.acceleration[0].Z = a.Z;
            this.radius = radius;

        }

        public override HitResult getIntersectionPoint(Ray r)
        {
            Vec3 L = r.origin - this.coordinates[0];
            Vec3 dir = r.direction;
            HitResult result = new HitResult();
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

        public Plane( Vec3 v, Vec3 a, Vec3 normal, Vec3 p0)
        {
            this.coordinates = new Vec3[1];
            this.velocity = new Vec3[1];
            this.acceleration = new Vec3[1];
            this.velocity[0].X = v.X;
            this.acceleration[0].X = a.X;
            this.velocity[0].Y = v.Y;
            this.acceleration[0].Y = a.Y;
            this.velocity[0].Z = v.Z;
            this.acceleration[0].Z = a.Z;
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
            return result;
            
        }
    }
    public class Disk : Object
    {
        public Vec3 normal { get; set; }
        public double radius { get; set; }
        public double square_radius { get; set; }

        public Disk(Vec3 center, Vec3 v, Vec3 a, Vec3 normal, double radius)
        {
            this.coordinates = new Vec3[1];
            this.velocity = new Vec3[1];
            this.acceleration = new Vec3[1];
            this.velocity[0].X = v.X;
            this.acceleration[0].X = a.X;
            this.velocity[0].Y = v.Y;
            this.acceleration[0].Y = a.Y;
            this.velocity[0].Z = v.Z;
            this.acceleration[0].Z = a.Z;
            this.normal = normal;
            this.radius = radius;
            this.square_radius = Math.Pow(radius,2);
            this.coordinates[0] = center;


        }
        public override HitResult getIntersectionPoint(Ray r)
        {
            Plane p = new Plane(new Vec3(), new Vec3(), normal, this.coordinates[0]);

            HitResult result = p.getIntersectionPoint(r);
            if(result.hit)
            {
                Vec3 v = result.point - this.coordinates[0];
                double d2 = v.dot(v);
                if (d2 <= square_radius)
                {
                    double denom = r.direction.dot(normal);
                    if (Math.Abs(denom) < 1e-6) { result.hit = false; return result; }
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
            else return result;
            

                
            
            

        }
    }

    public class Triangle : Object
    {
        
        public Triangle(Vec3[] coords, Vec3[] v, Vec3[] a, Vec3[] color, double[] opacity)
        {
            this.color = new Vec3[3];
            this.coordinates = new Vec3[3];
            this.velocity = new Vec3[3];
            this.acceleration = new Vec3[3];
            this.opacity = opacity;
            for (int i = 0; i < 3; i++)
            {
                this.color[i] = color[i];
                this.coordinates[i].X = coords[i].X;
                this.velocity[i].X = v[i].X;
                this.acceleration[i].X = a[i].X;
                this.coordinates[i].Y = coords[i].Y;
                this.velocity[i].Y = v[i].Y;
                this.acceleration[i].Y = a[i].Y;
                this.coordinates[i].Z = coords[i].Z;
                this.velocity[i].Z = v[i].Z;
                this.acceleration[i].Z = a[i].Z;
            }

        }

        public override HitResult getIntersectionPoint(Ray r)
        {
            throw new NotImplementedException();
        }
    }

}