using SkiaSharp;
using System.DirectoryServices.ActiveDirectory;
using System.Text.Json;
using static System.Windows.Forms.Design.AxImporter;

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


        public bool isBehind(Vec3 vertex)
        {
            return -vertex.Z < 0.1;
        }




        public void mapImage(bool IsAntiAlias = true)
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
            double ratio = (double)config.image_res[0] / (double)config.image_res[1];
            double startX = (0.5 / config.image_res[0] * 2 - 1) * ratio;
            double startY = 1 - (0.5 / config.image_res[1] * 2);
            double stepX = (2.0 / config.image_res[0]) * ratio;
            double stepY = -(2.0 / config.image_res[1]);

            // loop through each pixel in screen space
            Parallel.For(0, config.image_res[1], j =>
            {
                for (double i = 0; i < config.image_res[0]; i++)
                {
                    double sx = startX + i * stepX;
                    double sy = startY + j * stepY;

                    Vec4 dir4 = new Vec4(sx, sy, -1, 0);
                    dir4 = Globals.Camera.matrix * dir4;
                    dir4.normalize();
                    Vec3 dir = new Vec3
                    {
                        X = dir4.X,
                        Y = dir4.Y, 
                        Z = dir4.Z
                    };
                    Ray R = new Ray(camWorldPos, dir);
                    foreach(Object o in objects)
                    {
                        HitResult result = o.getIntersectionPoint(R);
                        int index = (int)((j * config.image_res[0] + i) * 4);
                        Vec4 pointCam = inverse * new Vec4(result.point.X, result.point.Y, result.point.Z, 1);
                        double depth = -pointCam.Z; 
                        if (result.hit && result.t > 0 && result.t < config.clipping_range && depth <= depths[(int)(j * config.image_res[0] + i)])
                        {
                        
                            if (o is Plane)
                            {
                                depths[(int)(j * config.image_res[0] + i)] = depth;
                                pixels[index + 0] = 0;
                                pixels[index + 1] = 255;
                                pixels[index + 2] = 0;
                                pixels[index + 3] = 255;
                                
                            }
                            else
                            {
                                depths[(int)(j * config.image_res[0] + i)] = depth;
                                pixels[index + 0] = 255;
                                pixels[index + 1] = 255;
                                pixels[index + 2] = 255;
                                pixels[index + 3] = 255;
                                
                            }
                            
                            
                            
                        }
                        

                        
                    }
                    
                }
            });


            //for (int i = 0; i < 4; i++)
            //{
            //    for (int j= 0; j < 4; j++)
            //    {
            //        Console.Write(Globals.Camera.matrix[i, j]);
            //    }
            //    Console.WriteLine();
            //}







            //Array.Clear(pixels, 0, pixels.Length);
            //Array.Fill(depths, double.PositiveInfinity);
            //Matrix4 inverse = Globals.Camera.matrix.InverseRotationPart();
            //foreach (Object obj in objects)
            //{
            //    if (obj is Triangle)
            //    {
            //        Vec3[] vertsproj = new Vec3[obj.coordinates.Length];
            //        Vec3[] colorss = new Vec3[obj.coordinates.Length];

            //        for (int i = 0; i < obj.coordinates.Length; i++)
            //        {
            //            Vec4 v = new Vec4(obj.coordinates[i].X, obj.coordinates[i].Y, obj.coordinates[i].Z, 1);
            //            v = inverse * v;
            //            vertsproj[i] = new Vec3(v.X, v.Y, v.Z);
            //            colorss[i] = obj.color[i];
            //        }
            //        List<Vec3[]> vertsClipped;
            //        List<Vec3[]> colorsClipped;
            //        (vertsClipped, colorsClipped) = clipVertices(vertsproj, colorss);
            //        for (int k = 0; k < vertsClipped.Count; k++)
            //        {
            //            Vec3[] vertices = new Vec3[]
            //            {
            //                projectVertex(vertsClipped[k][0]),
            //                projectVertex(vertsClipped[k][1]),
            //                projectVertex(vertsClipped[k][2])
            //            };

            //            (Vec3 min, Vec3 max) = getBoundingBox(vertices);

            //            int minX = Math.Max(0, (int)Math.Floor(min.X));
            //            int minY = Math.Max(0, (int)Math.Floor(min.Y));
            //            int maxX = Math.Min(config.image_res[0] - 1, (int)Math.Ceiling(max.X));
            //            int maxY = Math.Min(config.image_res[1] - 1, (int)Math.Ceiling(max.Y));


            //            bool behind = vertices.All(v => v.Z <= 0);

            //            bool offScreen = maxX < 0 || minX > config.image_res[0] - 1 ||
            //            maxY < 0 || minY > config.image_res[1] - 1;
            //            bool tooFar = vertices.All(v => v.Z > config.clipping_range);

            //            if (!offScreen && !tooFar && !behind)
            //            {


            //                double area = edgeFunction(vertices[0], vertices[1], vertices[2]);
            //                bool backFacing = area < 0;
            //                Vec3 c0 = colorsClipped[k][0];
            //                Vec3 c1 = colorsClipped[k][1];
            //                Vec3 c2 = colorsClipped[k][2];
            //                for (int i = minX; i <= maxX; i++)
            //                {
            //                    for (int j = minY; j <= maxY; j++)
            //                    {


            //                        Vec3 p = new Vec3(i + 0.5, j + 0.5, 0);
            //                        double w0 = edgeFunction(vertices[1], vertices[2], p);
            //                        double w1 = edgeFunction(vertices[2], vertices[0], p);
            //                        double w2 = edgeFunction(vertices[0], vertices[1], p);
            //                        double areab = area;
            //                        if (backFacing) { w0 = -w0; w1 = -w1; w2 = -w2; areab = -area; }

            //                        if (w0 >= 0 && w1 >= 0 && w2 >= 0)
            //                        {

            //                            double[] opacity = obj.opacity;

            //                            w0 /= areab;
            //                            w1 /= areab;
            //                            w2 /= areab;

            //                            double oneOverZ = w0 / vertices[0].Z + w1 / vertices[1].Z + w2 / vertices[2].Z;
            //                            double r = (w0 * c0.X / vertices[0].Z + w1 * c1.X / vertices[1].Z + w2 * c2.X / vertices[2].Z) / oneOverZ;
            //                            double g = (w0 * c0.Y / vertices[0].Z + w1 * c1.Y / vertices[1].Z + w2 * c2.Y / vertices[2].Z) / oneOverZ;
            //                            double b = (w0 * c0.Z / vertices[0].Z + w1 * c1.Z / vertices[1].Z + w2 * c2.Z / vertices[2].Z) / oneOverZ;
            //                            double o = (w0 * opacity[0] / vertices[0].Z + w1 * opacity[1] / vertices[1].Z + w2 * opacity[2] / vertices[2].Z) / oneOverZ;

            //                            double z = 1.0 / oneOverZ;
            //                            //if (z < 0) z = -z;

            //                            if (z <= depths[j * config.image_res[0] + i])
            //                            {
            //                                depths[j * config.image_res[0] + i] = z;
            //                                int index = (j * config.image_res[0] + i) * 4;
            //                                pixels[index + 0] = (byte)(b * o / 255);
            //                                pixels[index + 1] = (byte)(g * o / 255);
            //                                pixels[index + 2] = (byte)(r * o / 255);
            //                                pixels[index + 3] = (byte)(o);
            //                            }




            //                        }

            //                    }
            //                }


            //            }
            //        }
            //    }
            //}
        }

        public double edgeFunction(Vec3 a, Vec3 b, Vec3 c)
        {
            return (c.Y - a.Y) * (b.X - a.X) - (c.X - a.X) * (b.Y - a.Y);
        }
        public (List<Vec3[]>, List<Vec3[]>) clipVertices(Vec3[] verts, Vec3[] colors)
        {
            bool i0 = isBehind(verts[0]);
            bool i1 = isBehind(verts[1]);
            bool i2 = isBehind(verts[2]);
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


        public Vec3 projectVertex(Vec3 vertex)
        {



            Matrix4 projectionM = Matrix4.projectionMatrix();
            double f = 1 / Math.Tan(config.FOV / 2 / 57.2958D);



            Vec4 projected = new Vec4(vertex.X, vertex.Y, vertex.Z, 1);
            projected = projectionM * projected;
            double pRasterX = (projected.X + 1) / 2 * config.image_res[0];
            double pRasterY = (1 - projected.Y) / 2 * config.image_res[1];
            double pRasterZ = -vertex.Z;

            Vec3 v = new Vec3(pRasterX, pRasterY, pRasterZ);



            return v;

        }

    }
}
