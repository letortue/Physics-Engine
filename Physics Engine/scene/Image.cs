using OpenTK.Graphics.ES11;
using OpenTK.Graphics.ES20;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Physics_Engine
{
    public class Image
    {
        public Object[] objects;
        public byte[] pixels;
        public double[] depths;
        SKBitmap bitmap;
        Config config { get; set; }
        public Image(Object[] objects)
        {
            string json = File.ReadAllText("config.json");
            config = JsonSerializer.Deserialize<Config>(json)!;

            bitmap = new SKBitmap(config.image_res[0], config.image_res[1], SKColorType.Bgra8888, SKAlphaType.Premul);
            this.pixels = new byte[config.image_res[0] * config.image_res[1] * 4];
            this.depths = new double[config.image_res[0] * config.image_res[1]];
            Array.Fill(depths, double.PositiveInfinity);


            this.objects = objects;

            
            mapImage();
            

        }
        public (Vec3 min, Vec3 max) getBoundingBox(Vec3[] vertices)
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
        public void drawImage(SKCanvas canvas)
        {
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bitmap.GetPixels(), pixels.Length);
            canvas.DrawBitmap(bitmap, 0, 0);
        }
        public void update(Object o, double pixelsPerMeter = 50)
        {

            double t = (float)config.interval / 1000;
            
            for (int j = 0; j < o.coordinates.Length; j++)
            {

                o.velocity[j].X += t * o.acceleration[j].X;
                o.coordinates[j].X += t * o.velocity[j].X * pixelsPerMeter;
                o.velocity[j].Y += t * o.acceleration[j].Y;
                o.coordinates[j].Y += t * o.velocity[j].Y * pixelsPerMeter;
                o.velocity[j].Z += t * o.acceleration[j].Z;
                o.coordinates[j].Z += t * o.velocity[j].Z * pixelsPerMeter;

            }
        }


    

        public void mapImage(bool IsAntiAlias = true)
        {
            Array.Clear(pixels, 0, pixels.Length);
            Array.Fill(depths, double.PositiveInfinity);

            foreach (Object obj in objects)
            {
                Vec3[] vertices = projectVertices(obj);
                (Vec3 min, Vec3 max) = getBoundingBox(vertices);
                
                int minX = Math.Max(0, (int)Math.Floor(min.X));
                int minY = Math.Max(0, (int)Math.Floor(min.Y));
                int maxX = Math.Min(config.image_res[0] - 1, (int)Math.Ceiling(max.X));
                int maxY = Math.Min(config.image_res[1] - 1, (int)Math.Ceiling(max.Y));
                

                bool behind = vertices.All(v => v.Z <= 0);
                
                bool offScreen = maxX < 0 || minX > config.image_res[0] - 1 ||
                maxY < 0 || minY > config.image_res[1] - 1;
                bool tooFar = vertices.All(v => v.Z > config.clipping_range);

                if (!offScreen && !tooFar && !behind)
                {
                    if (obj is Triangle)
                    {

                        double area = edgeFunction(vertices[0], vertices[1], vertices[2]);
                        bool backFacing = area < 0;
                        Vec3 c0 = obj.color[0];
                        Vec3 c1 = obj.color[1];
                        Vec3 c2 = obj.color[2];
                        for (int i = minX; i <= maxX; i++)
                        {
                            for (int j = minY; j <= maxY; j++)
                            {

                                
                                Vec3 p = new Vec3(i + 0.5, j + 0.5, 0);
                                double w0 = edgeFunction(vertices[1], vertices[2], p);
                                double w1 = edgeFunction(vertices[2], vertices[0], p);
                                double w2 = edgeFunction(vertices[0], vertices[1], p);
                                if (backFacing) { w0 = -w0; w1 = -w1; w2 = -w2; area = -area; }
                                if (w0 >= 0 && w1 >= 0 && w2 >= 0)
                                {

                                    double[] opacity = obj.opacity;

                                    w0 /= area;
                                    w1 /= area;
                                    w2 /= area;
                                    double oneOverZ = w0 / vertices[0].Z + w1 / vertices[1].Z + w2 / vertices[2].Z;
                                    double r = (w0 * c0.X / vertices[0].Z + w1 * c1.X / vertices[1].Z + w2 * c2.X / vertices[2].Z) / oneOverZ;
                                    double g = (w0 * c0.Y / vertices[0].Z + w1 * c1.Y / vertices[1].Z + w2 * c2.Y / vertices[2].Z) / oneOverZ;
                                    double b = (w0 * c0.Z / vertices[0].Z + w1 * c1.Z / vertices[1].Z + w2 * c2.Z / vertices[2].Z) / oneOverZ;
                                    double o = (w0 * opacity[0] / vertices[0].Z + w1 * opacity[1] / vertices[1].Z + w2 * opacity[2] / vertices[2].Z) / oneOverZ;

                                    double z = 1.0 / oneOverZ;

                                    if (z <= depths[j* config.image_res[0] + i])
                                    {
                                        depths[j * config.image_res[0] + i] = z;
                                        int index = (j * config.image_res[0] + i) * 4;
                                        pixels[index + 0] = (byte)(b * o /255);
                                        pixels[index + 1] = (byte)(g * o/ 255);
                                        pixels[index + 2] = (byte)(r * o / 255);
                                        pixels[index + 3] = (byte)(o);
                                    }




                                }

                            }
                        }
                    }

                }

            }
        }
        
        public double edgeFunction(Vec3 a, Vec3 b, Vec3 c)
        {
            return  (c.Y - a.Y) * (b.X - a.X) - (c.X - a.X) * (b.Y - a.Y);
        }

        public Vec3[] projectVertices(Object o)
        {

            Vec3[] vertices = new Vec3[o.coordinates.Length];
            Matrix4 inverse = Globals.Camera.matrix.InverseRotationPart();

            for (int i = 0; i < o.coordinates.Length; i++)
            {


                Vec3 transformed = new Vec3(o.coordinates[i].X, o.coordinates[i].Y, o.coordinates[i].Z);
                transformed = transformed.WorldToCamera(inverse);


                //Vec3 relative_pos = new Vec3(Globals.Camera.matrix[0, 3], Globals.Camera.matrix[1, 3], Globals.Camera.matrix[2, 3]) - transformed;
                double x = transformed.X;
                double y = transformed.Y;
                double z = transformed.Z;

                //if (transformed.dot(new Vec3(0, 0, -1)) > 0 && relative_pos.Z < config.clipping_range)
                //{
                    double f = 1 / Math.Tan(config.FOV / 2 / 57.2958D);

                    double aspect_ratio = (double) config.image_res[0] / (double) config.image_res[1];

                    double pScreenX = (x * f / aspect_ratio) / -z;
                    double pScreenY = (y * f) / -z;


                    double l = -1;
                    double r = 1;
                    double b = -1;
                    double t = 1;
                    double pNDCX = 2 * pScreenX / (r - l) - (r + l) / (r - l);
                    double pNDCY = 2 * pScreenY / (t - b) - (t + b) / (t - b);

                    double pRasterX = (pNDCX + 1) / 2 * config.image_res[0];
                    double pRasterY = (1 - pNDCY) / 2 * config.image_res[1];
                    double pRasterZ = -z;

                    vertices[i] = new Vec3(pRasterX, pRasterY, pRasterZ);




                //}
                //else vertices[i] = null;

            }

            return vertices;

        }

    }
}
