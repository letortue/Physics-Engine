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
        public SceneObject o;
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
        public double[] opacity;
    }
    public struct ShadingAttributes
    {
        public bool lambertian;
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
            this.direction = direction.Normalize();
        }
        public override Vec3 GetDirection(Vec3 hitPoint)
        {
            return direction;
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
    public abstract class SceneObject
    {
        public Vec3[] vertices { get; set; }
        public VertexAttributes attributes;
        public ShadingAttributes shading;
        
    }


    public class Mesh : SceneObject
    {
        public int[] faces { get; set; }
        public int[] vertexIndices { get; set; }
        public bool[] onesided { get; set; }
        
        
        public int nTriangles { get; set; }
        public double[,] tIndices { get; set; }
        public Triangle[] triangles { get; set; }
        public int nfaces { get; set; }
        

        public Mesh(Vec3[] vertices, VertexAttributes attributes, ShadingAttributes shading, Vec3[] normals, int[] faces, int[] indices, bool clockwiseWinding = true)
        {
            this.vertices = vertices;
            this.faces = faces;
            this.vertexIndices = indices;
            this.attributes = attributes;
            this.shading = shading;

            this.nfaces = faces.Length;
            this.nTriangles = 0;

            for (int i = 0; i < nfaces; i++)
            {
                if(faces[i] > 2)
                {
                    this.nTriangles += (faces[i] - 2);
                }
            }
            this.tIndices = new double[nTriangles,3];

            List<Triangle> triangles = new List<Triangle>();
            int start = 0;
            int triIndex = 0;
            for (int i = 0; i < nfaces; i++)
            {
                for (int j = 0; (j + 2) < faces[i]; j++)
                {
                    int vi0, vi1, vi2;
                    if (!clockwiseWinding) { vi2 = vertexIndices[start]; vi1 = vertexIndices[start + j + 1]; vi0 = vertexIndices[start + j + 2]; }
                    else { vi0 = vertexIndices[start]; vi1 = vertexIndices[start + j + 1]; vi2 = vertexIndices[start + j + 2]; }
                    Vec3[] triangleVerts = { vertices[vi0], vertices[vi1], vertices[vi2] };
                    VertexAttributes atts = TVAtts(attributes, vi0, vi1, vi2);
                    ShadingAttributes sh = TShAtts(shading, i);
                    tIndices[triIndex, 0] = vi0;
                    tIndices[triIndex, 1] = vi1;
                    tIndices[triIndex, 2] = vi2;
                    Vec3 edge1 = vertices[vi1] - vertices[vi0];
                    Vec3 edge2 = vertices[vi2] - vertices[vi0];
                    Vec3 faceNormal = edge2.Cross(edge1).Normalize();
                    triangles.Add(new Triangle(triangleVerts, faceNormal, atts, sh));


                }
                start += faces[i];
                triIndex++;
            }
            this.triangles = [.. triangles];
        }

        
        public static VertexAttributes TVAtts(VertexAttributes attributes, int vi0, int vi1, int vi2)
        {
            VertexAttributes atts = attributes;
            atts.colors = [attributes.colors[vi0], attributes.colors[vi1], attributes.colors[vi2]];
            atts.velocity = [attributes.velocity[vi0], attributes.velocity[vi1], attributes.velocity[vi2]];
            atts.acceleration = [attributes.acceleration[vi0], attributes.acceleration[vi1], attributes.acceleration[vi2]];
            atts.opacity = [attributes.opacity[vi0], attributes.opacity[vi1], attributes.opacity[vi2]];
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
        
        
            
        
    }

    public class Ball : SceneObject
    {
        public double radius { get; set; }

        public Ball(Vec3 center, double radius, VertexAttributes attributes, ShadingAttributes shading)
        {
            
            this.attributes = attributes;
            this.vertices[0] = center;
            this.radius = radius;
            this.shading = shading;

        }

        


    }

    public class Plane : SceneObject
    {
        public Vec3 normal { get; set; }
        public Vec3 point { get; set; }

        public Plane( VertexAttributes attributes, ShadingAttributes shading, Vec3 normal, Vec3 point)
        {
            
            this.vertices[0] = point;
            this.attributes = attributes;
            this.normal = normal;
            this.point = point;
            this.shading = shading;
        }
        
    }
    public class Disk : SceneObject
    {
        public Vec3 normal { get; set; }
        public double radius { get; set; }
        public double square_radius { get; set; }
        public Plane p;

        public Disk(Vec3 center, VertexAttributes attributes,ShadingAttributes shading, Vec3 normal, double radius)
        {
            this.attributes= attributes;
            
            this.normal = normal;
            this.radius = radius;
            this.square_radius = Math.Pow(radius,2);
            this.vertices[0] = center;
            this.shading = shading;
            p = new Plane(attributes, shading, normal, this.vertices[0]);

        }
        
    }
    
    public class Triangle : SceneObject
    {
        public Vec3 normal { get; set; }
        public Triangle(Vec3[] vertices, Vec3 normal, VertexAttributes attributes, ShadingAttributes shading)
        {
            
            this.vertices = vertices;
            this.attributes = attributes;
            this.shading = shading;
            this.normal = normal;


        }

        
        
    }

}