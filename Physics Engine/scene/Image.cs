using SkiaSharp;
using System;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Text.Json;
using static System.Windows.Forms.Design.AxImporter;

namespace Physics_Engine
{
    public class Image
    {
        public Object[] objects;
        public Light[] lights;
        public byte[] pixels;
        public double[] depths;
        readonly SKBitmap bitmap;
        Config config { get; set; }
        Matrix4 ObjectToWorldMatrix { get; set; }
        public Image(Object[] objects, Light[] lights)
        {
            string json = File.ReadAllText("config.json");
            config = JsonSerializer.Deserialize<Config>(json)!;

            bitmap = new SKBitmap(config.Image_res[0], config.Image_res[1], SKColorType.Bgra8888, SKAlphaType.Premul);
            this.pixels = new byte[config.Image_res[0] * config.Image_res[1] * 4];
            this.depths = new double[config.Image_res[0] * config.Image_res[1]];
            Array.Fill(depths, double.PositiveInfinity);


            this.objects = objects;
            this.lights = lights;

            ObjectToWorldMatrix = new Matrix4();
            MapImage();


        }
        public (Vec3 min, Vec3 max) GetBoundingBox(Vec3[] vertices)
        {

            double MaxX = vertices[0].X;
            double MaxY = vertices[0].Y;
            double MaxZ = vertices[0].Z;
            double MinX = vertices[0].X;
            double MinY = vertices[0].Y;
            double MinZ = vertices[0].Z;
            foreach (Vec3 vertex in vertices)
            {
                if (vertex.X > MaxX) MaxX = vertex.X;
                if (vertex.Y > MaxY) MaxY = vertex.Y;
                if (vertex.Z > MaxZ) MaxZ = vertex.Z;
                if (vertex.X < MinX) MinX = vertex.X;
                if (vertex.Y < MinY) MinY = vertex.Y;
                if (vertex.Z < MinZ) MinZ = vertex.Z;
            }
            Vec3 min = new Vec3(MinX, MinY, MinZ);
            Vec3 max = new Vec3(MaxX, MaxY, MaxZ);
            return (min, max);
        }
        public void DrawImage(SKCanvas canvas)
        {
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bitmap.GetPixels(), pixels.Length);
            canvas.DrawBitmap(bitmap, 0, 0);
        }
        public void Update(Object o, double pixelsPerMeter = 50)
        {

            double t = (float)config.Interval / 1000;
            for (int j = 0; j < o.vertices.Length; j++)
            {
                
                
                o.attributes.velocity[j].X += t * o.attributes.acceleration[j].X;
                o.vertices[j].X += t * o.attributes.velocity[j].X * pixelsPerMeter;
                o.attributes.velocity[j].Y += t * o.attributes.acceleration[j].Y;
                o.vertices[j].Y += t * o.attributes.velocity[j].Y * pixelsPerMeter;
                o.attributes.velocity[j].Z += t * o.attributes.acceleration[j].Z;
                o.vertices[j].Z += t * o.attributes.velocity[j].Z * pixelsPerMeter;
                //ObjectToWorldMatrix[0, 3] += t * o.attributes.velocity[j].X * pixelsPerMeter;
                //ObjectToWorldMatrix[1, 3] += t * o.attributes.velocity[j].Y * pixelsPerMeter;
                //ObjectToWorldMatrix[2, 3] += t * o.attributes.velocity[j].Z * pixelsPerMeter;
                //Vec4 trans = ObjectToWorldMatrix * new Vec4(o.vertices[j].X, o.vertices[j].Y, o.vertices[j].Z, 1);
                //o.vertices[j].X = trans.X; o.vertices[j].Y = trans.Y; o.vertices[j].Z = trans.Z;


            }
            if (o is Mesh)
            {
                Mesh mesh = (Mesh)o;
                for (int j = 0;j < mesh.nTriangles; j++)
                {
                    mesh.triangles[j].vertices[0] = mesh.vertices[(int)mesh.tIndices[j,0]];
                    mesh.triangles[j].vertices[1] = mesh.vertices[(int)mesh.tIndices[j,1]];
                    mesh.triangles[j].vertices[2] = mesh.vertices[(int)mesh.tIndices[j,2]];
                }
            }
            


        }


        public static bool IsBehind(Vec3 vertex)
        {
            return -vertex.Z < 0.1;
        }




        public void MapImage(bool IsAntiAlias = true)
        {

            Matrix4 inverse = Globals.Camera.matrix.InverseRotationPart();
            Array.Clear(pixels, 0, pixels.Length);
            Array.Fill(depths, double.PositiveInfinity);
            Vec3 camWorldPos = new Vec3
            {
                X = Globals.Camera.matrix[0, 3],
                Y = Globals.Camera.matrix[1, 3],
                Z = Globals.Camera.matrix[2, 3],
            };
            
            double ratio = (double)config.Image_res[0] / (double)config.Image_res[1];
            double startX = (0.5 / config.Image_res[0] * 2 - 1) * ratio;
            double startY = 1 - (0.5 / config.Image_res[1] * 2);
            double stepX = (2.0 / config.Image_res[0]) * ratio;
            double stepY = -(2.0 / config.Image_res[1]);

            // loop through each pixel in screen space

            Parallel.For(0, config.Image_res[1], j =>
            {
                for (double i = 0; i < config.Image_res[0]; i++)
                {
                    double sx = startX + i * stepX;
                    double sy = startY + j * stepY;

                    Vec4 dir4 = new Vec4(sx, sy, -1, 0);
                    dir4 = Globals.Camera.matrix * dir4;
                    dir4.Normalize();
                    Vec3 dir = new Vec3
                    {
                        X = dir4.X,
                        Y = dir4.Y,
                        Z = dir4.Z
                    };
                    Ray R = new Ray(camWorldPos, dir);

                    MapObjects( R, i, j);
                }
            });

 
        }

        private void MapObjects( Ray R, double i, double j)
        {
            
            Vec3 minusDir = new Vec3(0 - R.direction.X, 0 - R.direction.Y, 0 - R.direction.Z);
            minusDir = minusDir.Normalize();
            foreach (Object o in objects)
            {
                if (o is Mesh) { HandleMesh((Mesh)o, R, i, j); continue; };
                HitResult result = o.GetIntersectionPoint(R);
                int index = (int)((j * config.Image_res[0] + i) * 4);
                double depth = result.t;


                if (!result.hit || result.t <= 0 || result.t > config.Clipping_range || depth > depths[(int)(j * config.Image_res[0] + i)]) { continue; }
                
                depths[(int)(j * config.Image_res[0] + i)] = depth;
                    
                    
                if (o.shading.facing_ratio[0])
                {
                    double minus = minusDir.Dot(result.normal);

                    double facingIntesity;
                    facingIntesity = Math.Max(minus, 0);

                    pixels[index + 0] = (byte)(facingIntesity * result.color.Z);
                    pixels[index + 1] = (byte)(facingIntesity * result.color.Y);
                    pixels[index + 2] = (byte)(facingIntesity * result.color.X);
                }
                else
                {
                    Vec3 albedo;
                    if (o.shading.interpolatedAlbedo)
                    {
                        albedo = result.albedo;
                    }
                    else
                    {
                        albedo = o.shading.albedo[0];
                    }
                    Vec3 color = GetSurfaceColor(result, this.lights, albedo);
                    byte colorX = (byte) Math.Clamp(color.X, 0, 255);
                    byte colorY = (byte) Math.Clamp(color.Y, 0, 255);
                    byte colorZ = (byte) Math.Clamp(color.Z, 0, 255);
                    pixels[index + 0] = (byte)(colorZ);
                    pixels[index + 1] = (byte)(colorY);
                    pixels[index + 2] = (byte)(colorX);
                    pixels[index + 3] = 255;
                }





                



            }
        }
        public void HandleMesh(Mesh mesh, Ray R, double i, double j)
        {
            Vec3 minusDir = new Vec3(0 - R.direction.X, 0 - R.direction.Y, 0 - R.direction.Z);
            minusDir = minusDir.Normalize();
            int index = (int)((j * config.Image_res[0] + i) * 4);
            foreach (Triangle t in mesh.triangles)
            {
                HitResult result = t.GetIntersectionPoint(R);
                double depth = result.t;


                if (!result.hit || result.t <= 0 || result.t > config.Clipping_range || depth > depths[(int)(j * config.Image_res[0] + i)]) { continue; }
                
                depths[(int)(j * config.Image_res[0] + i)] = depth;

                
                if (t.shading.facing_ratio[0])
                {
                    double minus = minusDir.Dot(result.normal);
                    double facingIntesity;
                    if (!t.onesided) facingIntesity = Math.Max(Math.Abs(minus), 0);
                    else facingIntesity = Math.Max(minus, 0);

                    pixels[index + 0] = (byte)(facingIntesity * result.color.Z);
                    pixels[index + 1] = (byte)(facingIntesity * result.color.Y);
                    pixels[index + 2] = (byte)(facingIntesity * result.color.X);
                    pixels[index + 3] = 255;
                }
                else
                {
                    Vec3 albedo;
                    if(t.shading.interpolatedAlbedo)
                    {
                       albedo = result.albedo; 
                    }
                    else
                    {
                        albedo = t.shading.albedo[0];
                    }
                    Vec3 color = GetSurfaceColor(result, this.lights, albedo);
                    byte colorX = (byte)Math.Clamp(color.X, 0, 255);
                    byte colorY = (byte)Math.Clamp(color.Y, 0, 255);
                    byte colorZ = (byte)Math.Clamp(color.Z, 0, 255);
                    pixels[index + 0] = (byte)(colorZ);
                    pixels[index + 1] = (byte)(colorY);
                    pixels[index + 2] = (byte)(colorX);
                    pixels[index + 3] = 255;
                }





                
            }
        }
        public Vec3 GetSurfaceColor(HitResult hit, Light[] lights, Vec3 albedoo)
        {
            Vec3 albedo = albedoo / Math.PI;
            Vec3 contributions = new Vec3(0, 0, 0);
            foreach (Light light in lights)
            {
                Vec3 dir = light.GetDirection(hit.point);
                Vec3 minusDir = new Vec3(0 - dir.X, 0 - dir.Y, 0 - dir.Z);

                contributions += Math.Max(minusDir.Dot(hit.normal), 0) * light.GetIntensity(hit.point) * light.color;

            }
           

            return albedo * contributions;

        }
                  
        

        public static double EdgeFunction(Vec3 a, Vec3 b, Vec3 c)
        {
            return (c.Y - a.Y) * (b.X - a.X) - (c.X - a.X) * (b.Y - a.Y);
        }
        public static (List<Vec3[]>, List<Vec3[]>) clipVertices(Vec3[] verts, Vec3[] colors)
        {
            bool i0 = IsBehind(verts[0]);
            bool i1 = IsBehind(verts[1]);
            bool i2 = IsBehind(verts[2]);
            int count = (i0 ? 1 : 0) + (i1 ? 1 : 0) + (i2 ? 1 : 0);
            if (count == 3) return (new List<Vec3[]>(), new List<Vec3[]>());

            if (count == 0)
            {
                return (new List<Vec3[]> { verts }, new List<Vec3[]> { colors });
            }
            if (count == 2)
            {
                Vec3 a, b, c;
                Vec3 ca, cb, cc;
                if (!i0)
                {
                    a = verts[0]; b = verts[1]; c = verts[2];
                    ca = colors[0]; cb = colors[1]; cc = colors[2];
                }
                else
                if (!i1)
                {
                    a = verts[1]; b = verts[2]; c = verts[0];
                    ca = colors[1]; cb = colors[2]; cc = colors[0];
                }
                else
                {
                    a = verts[2]; b = verts[0]; c = verts[1];
                    ca = colors[2]; cb = colors[0]; cc = colors[1];
                }
                Vec3 ab, ac, abColor, acColor;
                //(ab, abColor) = getIntersectionPoint(a, b, ca, cb);
                //(ac, acColor) = getIntersectionPoint(a, c, ca, cc);
                //return (new List<Vec3[]> { new Vec3[] { a, ab, ac } }, new List<Vec3[]> { new Vec3[] { ca, abColor, acColor } });
                return (new List<Vec3[]>(), new List<Vec3[]>());
            }
            else
            {
                Vec3 a, b, c;
                Vec3 ca, cb, cc;
                if (i0)
                {
                    a = verts[0]; b = verts[1]; c = verts[2];
                    ca = colors[0]; cb = colors[1]; cc = colors[2];
                }
                else
                if (i1)
                {
                    a = verts[1]; b = verts[2]; c = verts[0];
                    ca = colors[1]; cb = colors[2]; cc = colors[0];
                }
                else
                {
                    a = verts[2]; b = verts[0]; c = verts[1];
                    ca = colors[2]; cb = colors[0]; cc = colors[1];
                }
                Vec3 ab, ac, abColor, acColor;
                //(ab, abColor) = getIntersectionPoint(a, b, ca, cb);
                //(ac, acColor) = getIntersectionPoint(a, c, ca, cc);
                //return (new List<Vec3[]> { new Vec3[] { ab, b, c }, new Vec3[] { ab, c, ac } }, new List<Vec3[]> { new Vec3[] { abColor, cb, cc }, new Vec3[] { abColor, cc, acColor } });
                return (new List<Vec3[]>(), new List<Vec3[]>());
            }
        }


        public Vec3 ProjectVertex(Vec3 vertex)
        {



            Matrix4 projectionM = Matrix4.ProjectionMatrix();
            double f = 1 / Math.Tan(config.FOV / 2 / 57.2958D);



            Vec4 projected = new Vec4(vertex.X, vertex.Y, vertex.Z, 1);
            projected = projectionM * projected;
            double pRasterX = (projected.X + 1) / 2 * config.Image_res[0];
            double pRasterY = (1 - projected.Y) / 2 * config.Image_res[1];
            double pRasterZ = -vertex.Z;

            Vec3 v = new Vec3(pRasterX, pRasterY, pRasterZ);



            return v;

        }

    }
}
