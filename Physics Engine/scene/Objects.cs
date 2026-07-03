using Accessibility;
using Aspose.ThreeD;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Physics_Engine
{
    public struct HitResult
    {
        public bool hit;
        public Vec3 point;
        public Vec3 normal;
        public Vec3 color;
        public Vec3 albedo;
        public double t;
        public double w0;
        public double w1;
        public double w2;
        public Object o;
        public double textureT;
        public double textureV;
        
    }
    public struct VertexAttributes
    {
        public Vec3[] colors;
        public Vec3[] albedo;
        public Vec3[] velocity;
        public Vec3[] acceleration;
        public double[] textureT;
        public double[] textureV;
    }
    public struct ShadingAttributes
    {
        public bool isInterpolatedAlbedo;
        public Vec3[] albedo;
        public bool[] facing_ratio;
        public bool oneSided;
        public bool isReflective;
        public bool isRefractive;
        public double refIndex;
        public Func<Vec3, Vec3> textureFunc;
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
    public abstract class Light
    {
        public Vec3 color { get; set; }
        protected double intensity;
        public abstract Vec3 GetDirection(Vec3 hitPoint);
        public abstract double GetIntensity(Vec3 hitPoint);
    }

    public class PointLight : Light
    {
        public Vec3 pos { get; set; }
        public PointLight(Vec3 pos, Vec3 color, double intensity)
        {
            this.pos = pos;
            this.color = color;
            this.intensity = intensity;
            
        }
        public override Vec3  GetDirection(Vec3 hitPoint)
        {
            return (hitPoint - pos).Normalize();
        }
        public override double GetIntensity(Vec3 hitPoint)
        {
            double distance = (pos - hitPoint).Magnitude();
            return intensity / (distance * distance * 4 * Math.PI);
        }
    }
    public class DistantLight : Light
    {
        Vec3 direction;
        public DistantLight( Vec3 color, double intensity, Vec3 direction)
        {
            this.color = color;
            this.intensity = intensity;
            this.direction = direction;
        }
        public override Vec3 GetDirection(Vec3 hitPoint)
        {
            Vec3 dir = direction.Normalize();
            return direction.Normalize();
        }
        public override double GetIntensity(Vec3 hitPoint)
        {
            
            return intensity;
        }

    }
    public class SpotLight: Light
    {
        Vec3 pos { get; set; }
        public double falloff { get; set; }
        public Vec3 facingDir { get; set; }
        public SpotLight(Vec3 pos, Vec3 color, double intensity, double falloff, Vec3 facingDir)
        {
            this.color = color;
            this.pos = pos; 
            this.intensity = intensity;
            this.falloff = falloff;
            this.facingDir = facingDir.Normalize();
        }
        public override Vec3 GetDirection(Vec3 hitPoint)
        {
            return (hitPoint - pos).Normalize();
        }
        public override double GetIntensity(Vec3 hitPoint)
        {
            double distance = (pos - hitPoint).Magnitude();
            return intensity / (distance * distance * 4 * Math.PI);
        }
    }
    public abstract class Object
    {
        public Vec3[] vertices { get; set; }
        
        public VertexAttributes attributes;
        public ShadingAttributes shading;
        public abstract HitResult GetIntersectionPoint(Ray r);
    }


    public class Mesh : Object
    {
        public int[] faces { get; set; }
        public int nfaces { get; set; }
        public int[] vertexIndices { get; set; }
        public bool convex { get; set; }
        public bool[] onesided { get; set; }
        public Triangle[] triangles { get; set; }
        public int nTriangles { get; set; }
        public double[,] tIndices { get; set; }
        public Mesh(Vec3[] vertices, VertexAttributes attributes, ShadingAttributes shading, int[] faces, int[] indices, bool[] onesided, bool imported = false, Vec3[] faceNormals = null, int[] faceNormalIndices = null)
        {
            this.vertices = vertices;
            this.faces = faces;
            this.nfaces = faces.Length;
            this.vertexIndices = indices;
            this.attributes = attributes;
            this.onesided= onesided;
            this.shading = shading;
            this.nTriangles = 0;
            for (int i = 0; i < faces.Length; i++)
            {
                if(faces[i] > 2)
                {
                    this.nTriangles += (faces[i] - 2);
                }
            }
            this.tIndices = new double[nTriangles,3];

            triangles = new Triangle[this.nTriangles];
            int start = 0;
            int triIndex = 0;
            for (int i = 0; i < nfaces; i++)
            {
                    for (int j = 0; (j + 2) < faces[i]; j++)
                    { 
                        int vi0 = vertexIndices[start]; int vi1 = vertexIndices[start + j + 1]; int vi2 = vertexIndices[start + j + 2];
                        Vec3[] triangleVerts = {  vertices[vi0], vertices[vi1], vertices[vi2] };
                        VertexAttributes atts = TVAtts(attributes, vi0, vi1, vi2);
                        ShadingAttributes sh = TShAtts(shading, i);
                        tIndices[triIndex, 0] = vi0;
                        tIndices[triIndex, 1] = vi1;
                        tIndices[triIndex, 2] = vi2;
                        triangles[triIndex++] = new Triangle(triangleVerts, atts,sh, onesided[i]);
                        
                       
                    }
                start += faces[i];
            }
            
        }
        public static VertexAttributes TVAtts(VertexAttributes attributes, int vi0, int vi1, int vi2)
        {
            VertexAttributes atts = attributes;
            atts.colors = [attributes.colors[vi0], attributes.colors[vi1], attributes.colors[vi2]];
            atts.velocity = [attributes.velocity[vi0], attributes.velocity[vi1], attributes.velocity[vi2]];
            atts.acceleration = [attributes.acceleration[vi0], attributes.acceleration[vi1], attributes.acceleration[vi2]];
            
            atts.albedo = [attributes.albedo[vi0], attributes.albedo[vi1], attributes.albedo[vi2]];
            atts.textureT = [attributes.textureT[vi0], attributes.textureT[vi1], attributes.textureT[vi2]];
            atts.textureV = [attributes.textureV[vi0], attributes.textureV[vi1], attributes.textureV[vi2]];
            return atts;
        }
        public static ShadingAttributes TShAtts(ShadingAttributes attributes, int i)
        {
            attributes.facing_ratio = [attributes.facing_ratio[i]];
            attributes.albedo = [attributes.albedo[i]];
            
            return attributes;
        }
        public override HitResult GetIntersectionPoint(Ray r)
        {
            HitResult closest = new HitResult { hit = false };
            foreach (Triangle tri in triangles)
            {
                HitResult result = tri.GetIntersectionPoint(r);
                if (result.hit && (!closest.hit || result.t < closest.t))
                    closest = result;
            }
            return closest;
            // onesided bug stems from the fact that some faces' vertices are defined in the wrong order -
            // the order has to be clockwise according to the original coordinate system
        }
    }

    public class Ball : Object
    {
        public double radius { get; set; }

        public Ball(Vec3 coords, VertexAttributes attributes, ShadingAttributes shading, double radius)
        {
            this.vertices = new Vec3[1];
            this.attributes = attributes;
            this.vertices[0].X = coords.X;
            this.vertices[0].Y = coords.Y;
            this.vertices[0].Z = coords.Z;
            this.radius = radius;
            this.shading = shading;

        }

        public override HitResult GetIntersectionPoint(Ray r)
        {

            Vec3 L = r.origin - this.vertices[0];
            Vec3 dir = r.direction;
            HitResult result = new HitResult();
            result.color = this.attributes.colors[0];
            
            result.o = this;
            bool intersects = Maths.SolveQuadratic(dir.Dot(dir), 2 * L.Dot(dir), L.Dot(L) - Math.Pow(this.radius, 2), out double? t1, out double? t2);
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
                    result.albedo = this.shading.textureFunc is not null ? this.shading.textureFunc(result.point) : this.shading.albedo[0];
                    result.normal = (result.point - this.vertices[0]).Normalize();
                    result.t = (double) t1;
                    return result;
                }
                else
                {
                    if(t1< t2)
                    {
                        result.point = r.origin + (double)t1 * dir;
                        result.albedo = this.shading.textureFunc is not null ? this.shading.textureFunc(result.point) : this.shading.albedo[0];
                        result.normal = (result.point - this.vertices[0]).Normalize();
                        result.t = (double)t1;
                    }
                    else
                    {
                        result.point = r.origin + (double)t2 * dir;
                        result.albedo = this.shading.textureFunc is not null ? this.shading.textureFunc(result.point) : this.shading.albedo[0];
                        result.normal = (result.point - this.vertices[0]).Normalize();
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
        public Vec3 point { get; set; }

        public Plane( VertexAttributes attributes, ShadingAttributes shading, Vec3 normal, Vec3 point)
        {
            this.vertices = new Vec3[1];
            this.vertices[0] = point;
            this.attributes = attributes;
            this.normal = normal;
            this.point = point;
            this.shading = shading;
        }
        public override HitResult GetIntersectionPoint(Ray r)
        {
            HitResult result = new HitResult();
            double denom = r.direction.Dot(normal);
            if (Math.Abs(denom) < 1e-6) { result.hit = false; return result; }
            result.t = (point - r.origin).Dot(normal) / denom;
            if (result.t < 0) { result.hit = false; return result; }
            result.normal = normal;
            result.o = this;
            result.hit = true;
            result.point = r.origin + (result.t * r.direction);
            result.color = this.attributes.colors[0];
            result.albedo = this.shading.textureFunc is not null ? this.shading.textureFunc(result.point) : this.shading.albedo[0];
            
            
            return result;
            
        }
    }
    public class Disk : Object
    {
        public Vec3 normal { get; set; }
        public double radius { get; set; }
        public double square_radius { get; set; }
        private Plane p;

        public Disk(Vec3 center, VertexAttributes attributes,ShadingAttributes shading, Vec3 normal, double radius)
        {
            this.attributes= attributes;
            this.vertices = new Vec3[1];
            this.normal = normal;
            this.radius = radius;
            this.square_radius = Math.Pow(radius,2);
            this.vertices[0] = center;
            this.shading = shading;
            p = new Plane(attributes, shading, normal, this.vertices[0]);

        }
        public override HitResult GetIntersectionPoint(Ray r)
        {
            

            HitResult result = p.GetIntersectionPoint(r);
            double denom = r.direction.Dot(normal);
            if (Math.Abs(denom) < 1e-6) { result.hit = false; return result; }
            result.albedo = this.shading.textureFunc is not null ? this.shading.textureFunc(result.point) : this.shading.albedo[0];
            result.color = this.attributes.colors[0];
            result.normal = normal;
            result.o = this;
            result.textureT = result.point.X;
            result.textureV = result.point.Y;
            if (!result.hit)
            {
                return result;
            }

            Vec3 v = result.point - this.vertices[0];
            double d2 = v.Dot(v);

            if (d2 <= square_radius)
            {
                
                result.t = (this.vertices[0] - r.origin).Dot(normal) / denom;
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
    
    public class Triangle : Object
    {
        public Vec3 normal;
        
        public bool onesided;
        private double area;
        
        
        
        public Triangle(Vec3[] coords, VertexAttributes attributes, ShadingAttributes shading, bool onesided = false)
        {

            this.vertices = new Vec3[3];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].X = coords[i].X;
                vertices[i].Y = coords[i].Y;
                vertices[i].Z = coords[i].Z;
            }
            this.area = (vertices[1] - vertices[0]).Cross(vertices[2] - vertices[0]).Magnitude();
            this.normal = (vertices[1] - vertices[0]).Cross(vertices[2] - vertices[0]).Normalize();
            
            this.onesided = onesided;
            this.attributes = attributes;
            this.shading = shading;
            
                

        }

        public override HitResult GetIntersectionPoint(Ray r)
        {
            Vec3 tuv = Maths.CramersRule(r, this.vertices);
            double w0 = 1 - tuv[1] - tuv[2];
            double w1 = tuv[1];
            double w2 = tuv[2];
            if (w0 < 0 || w1 < 0 || w2 < 0 || w0 > 1 || w1 > 1 || w2 > 1) { return new HitResult { hit = false }; }

            if ((onesided && normal.Dot(r.direction) > 0) || Math.Abs(normal.Dot(r.direction)) < 1e-6 || tuv[0] < 0) return new HitResult { hit = false };
            
            Vec3 point = r.origin + tuv[0] * r.direction;
            HitResult result = new HitResult
            {
                hit = true,
                point = point,
                normal = normal,
                t = tuv[0],
                w0 = w0,
                w1 = w1,
                w2 = w2,
                o = this
            };
            Vec3 offsetNormal = r.direction.Dot(normal) < 0 ? normal : 0 - normal;
            result.point = point + 1e-4 * offsetNormal;
            result.normal = offsetNormal;
            double R = attributes.colors[0].X * w0 + attributes.colors[1].X * w1 + attributes.colors[2].X * w2;
            double G = attributes.colors[0].Y * w0 + attributes.colors[1].Y * w1 + attributes.colors[2].Y * w2;
            double B = attributes.colors[0].Z * w0 + attributes.colors[1].Z * w1 + attributes.colors[2].Z * w2;
            result.color.X = R; result.color.Y = G; result.color.Z = B;
            
            if (this.shading.textureFunc is not null)
            {
                result.textureT = attributes.textureT[0] * w0 + attributes.textureT[1] * w1 + attributes.textureT[2] * w2;
                result.textureV = attributes.textureV[0] * w0 + attributes.textureV[1] * w1 + attributes.textureV[2] * w2;
                result.albedo = this.shading.textureFunc(result.point);
                return result;
            }

            
            if (shading.isInterpolatedAlbedo)
            {
                double albedoR = attributes.albedo[0].X * w0 + attributes.albedo[1].X * w1 + attributes.albedo[2].X * w2;
                double albedoG = attributes.albedo[0].Y * w0 + attributes.albedo[1].Y * w1 + attributes.albedo[2].Y * w2;
                double albedoB = attributes.albedo[0].Z * w0 + attributes.albedo[1].Z * w1 + attributes.albedo[2].Z * w2;
                result.albedo.X = albedoR;
                result.albedo.Y = albedoG;
                result.albedo.Z = albedoB;
            }
            else
                result.albedo = shading.albedo[0];

            



            return result;

            



        }
        
    }

}