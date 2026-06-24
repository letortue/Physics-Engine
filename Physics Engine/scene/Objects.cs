using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Physics_Engine
{
    public struct VertexAttributes
    {
        public Vec3[] colors;
        public Vec3[] velocity;
        public Vec3[] acceleration;
        public double[] opacity;

    }
    public struct ShadingAttributes
    {
        public Vec3[] normal;
        public bool lambertian;
        public bool onesided;
    }
    public abstract class Object
    {
        public Vec3[] vertices { get; set; }
        public VertexAttributes attributes { get; set; }
        public ShadingAttributes shading { get; set; }

    }


    public abstract class Light
    {
        
        public abstract Vec3 GetDirection(Vec3 point);
        public abstract double GetIntensity(Vec3 normal);
    }

    public class DistantLight : Light
    {
        Vec3 direction;
        Vec3 negativeDir;
        double intensity;
        Config config { get; set; }

        public DistantLight(Vec3 direction, double intensity)
        {
            string json = File.ReadAllText("config.json");
            config = JsonSerializer.Deserialize<Config>(json)!;
            this.direction = direction.Normalize();
            this.intensity = intensity;
            negativeDir = new Vec3(0-this.direction.X, 0-this.direction.Y, 0-this.direction.Z);
        }
        public override Vec3 GetDirection(Vec3 point)
        {
            return this.direction;
        }
        public override double GetIntensity(Vec3 normal)
        {

            return Math.Max(0,(this.intensity / config.LightDampener) * normal.Dot(negativeDir)) ;
        }
    }
    public class Ball : Object
    {
        public double radius { get; set; }
        public Ball(Vec3 center, double radius, VertexAttributes attributes, ShadingAttributes shading)
        {
            this.vertices = new Vec3[1];
            this.vertices[0] = center;
            this.attributes = attributes;
            this.radius = radius;
            this.shading = shading;
        }



    }

    public class Triangle : Object
    {
        public Vec3 normal { get; set; }
        public Triangle(Vec3[] vertices,Vec3 normal, VertexAttributes attributes, ShadingAttributes shading)
        {
            this.vertices = new Vec3[3];
            this.vertices = vertices;
            this.attributes = attributes;
            this.shading = shading;
            this.normal = normal;
        }
    }
    public class Mesh : Object
    {
        public int[] faces { get; set; }
        public int nfaces { get; set; }
        public int[] vertexIndices { get; set; }
        public bool convex { get; set; }
        public Triangle[] triangles { get; set; }
        public int nTriangles { get; set; }
        public double[,] tIndices { get; set; }
        public Vec3[] normals { get; set; }
        
        public Mesh(Vec3[] vertices, VertexAttributes attributes, ShadingAttributes shading, Vec3[] normals, int[] faces, int[] indices)
        {
            this.vertices = vertices;
            this.faces = faces;
            this.nfaces = faces.Length;
            this.vertexIndices = indices;
            this.attributes = attributes;
            this.shading = shading;
            this.normals = normals;

            this.nTriangles = 0;
            for (int i = 0; i < nfaces; i++)
            {
                if (faces[i] > 2)
                {
                    this.nTriangles += (faces[i] - 2);
                }
            }
            this.tIndices = new double[nTriangles, 3];
            
            List<Triangle> triangles = new List<Triangle>();
            int start = 0;
            int triIndex = 0;
            for (int i = 0; i < nfaces; i++)
            {
                for (int j = 0; (j + 2) < faces[i]; j++)
                {
                    int vi0 = vertexIndices[start]; int vi1 = vertexIndices[start + j + 1]; int vi2 = vertexIndices[start + j + 2];
                    Vec3[] triangleVerts = { vertices[vi2], vertices[vi1], vertices[vi0] };
                    VertexAttributes atts = TVAtts(attributes, vi2, vi1, vi0);
                    ShadingAttributes sh = TShAtts(shading, i);
                    tIndices[triIndex, 0] = vi2;
                    tIndices[triIndex, 1] = vi1;
                    tIndices[triIndex, 2] = vi0;
                    Vec3 edge1 = vertices[vi1] - vertices[vi0];
                    Vec3 edge2 = vertices[vi2] - vertices[vi0];
                    Vec3 faceNormal = edge2.Cross(edge1).Normalize();
                    
                    triangles.Add(new Triangle(triangleVerts, faceNormal, atts, sh));
                    

                }
                start += faces[i];
            }
            this.triangles = triangles.ToArray();
        }
        public static VertexAttributes TVAtts(VertexAttributes attributes, int vi0, int vi1, int vi2)
        {
            VertexAttributes atts = attributes;
            try
            {
                atts.colors = [attributes.colors[vi0], attributes.colors[vi1], attributes.colors[vi2]];
                atts.velocity = [attributes.velocity[vi0], attributes.velocity[vi1], attributes.velocity[vi2]];
                atts.acceleration = [attributes.acceleration[vi0], attributes.acceleration[vi1], attributes.acceleration[vi2]];
                atts.opacity = [attributes.opacity[vi0], attributes.opacity[vi1], attributes.opacity[vi2]];
            }
            catch (Exception e)
            {
                atts.colors = [new Vec3(255, 255, 255), new Vec3(255, 255, 255), new Vec3(255, 255, 255)];
                atts.velocity = [new Vec3(0, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 0)];
                atts.acceleration = [new Vec3(0, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 0)];
                atts.opacity = [255,255,255];
                
            }
            
            
            return atts;
        }
        public static ShadingAttributes TShAtts(ShadingAttributes attributes, int i)
        {

            return attributes;
        }
    }
}