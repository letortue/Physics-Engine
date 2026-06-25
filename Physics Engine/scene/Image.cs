using Accessibility;
using OpenTK.Graphics.ES11;
using SkiaSharp;
using System.Text.Json;
using static OpenTK.Graphics.OpenGL.GL;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Physics_Engine
{
    public class Image
    {
        public Object[] objects;
        public Light[] lights;
        public double[] depths;
        SKBitmap bitmap;
        Config config { get; set; }
        public Image(Object[] objects, Light[] lights)
        {
            string json = File.ReadAllText("config.json");
            config = JsonSerializer.Deserialize<Config>(json)!;

            bitmap = new SKBitmap(config.Image_Res[0], config.Image_Res[1], SKColorType.Bgra8888, SKAlphaType.Premul);
            
            this.depths = new double[config.Image_Res[0] * config.Image_Res[1]];
            Array.Fill(depths, double.PositiveInfinity);


            this.objects = objects;
            this.lights = lights;
            
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
        public void DrawImage(SKCanvas canvas, SKBitmap bitmap)
        {
            
            canvas.DrawBitmap(bitmap, 0, 0);
        }
        public void Update(Object o, double pixelsPerMeter = 50)
        {
            if (o is Mesh mesh)
            {
                foreach(Triangle triangle in mesh.triangles) this.Update(triangle, pixelsPerMeter);
                return;
            }
            double t = (float)config.Interval / 1000;
            
            for (int j = 0; j < o.vertices.Length; j++)
            {

                o.attributes.velocity[j].X += t * o.attributes.acceleration[j].X;
                o.vertices[j].X += t * o.attributes.velocity[j].X * pixelsPerMeter;
                o.attributes.velocity[j].Y += t * o.attributes.acceleration[j].Y;
                o.vertices[j].Y += t * o.attributes.velocity[j].Y * pixelsPerMeter;
                o.attributes.velocity[j].Z += t * o.attributes.acceleration[j].Z;
                o.vertices[j].Z += t * o.attributes.velocity[j].Z * pixelsPerMeter;

            }
        }


        

        public byte[] MapImage(bool IsAntiAlias = true)
        {
            byte[] pixels = new byte[config.Image_Res[0] * config.Image_Res[1] * 4];
            Array.Fill(depths, double.PositiveInfinity);
            Matrix4 inverse = Globals.Camera.matrix.InverseRotationPart();
            foreach (Object obj in objects)
            {
                if (obj is Triangle) HandleTriangle(pixels,(Triangle) obj, inverse);  
                if(obj is Mesh) HandleMesh(pixels,(Mesh) obj, inverse);
                
                    
                
            }
            return pixels;
        }
        
        public void HandleMesh(byte[] pixels, Mesh mesh, Matrix4 inverse)
        {
            
            foreach (Triangle t in mesh.triangles)
            {
                Vec3 centroid = (t.vertices[0] + t.vertices[1] + t.vertices[2]) / 3.0;
                Vec3 viewDir = (centroid - new Vec3(Globals.Camera.matrix[0, 3], Globals.Camera.matrix[1, 3], Globals.Camera.matrix[2, 3])).Normalize();

                if (viewDir.Dot(t.normal) > 0 && mesh.shading.onesided == true)  continue; 
                ;
                
                HandleTriangle(pixels, t, inverse);

            }
        }
        public void HandleTriangle(byte[] pixels, Triangle t, Matrix4 inverse)
        {
            
                Vec3[] vertsproj = new Vec3[t.vertices.Length];
                Vec3[] colors = new Vec3[t.vertices.Length];

                for (int i = 0; i < t.vertices.Length; i++)
                {
                    Vec4 v = new Vec4(t.vertices[i].X, t.vertices[i].Y, t.vertices[i].Z, 1);
                    v = inverse * v;
                    vertsproj[i] = new Vec3(v.X, v.Y, v.Z);
                    colors[i] = t.attributes.colors[i];
                }
                List<Vec3[]> vertsClipped;
                List<Vec3[]> colorsClipped;
                (vertsClipped, colorsClipped) = ClipVertices(vertsproj, colors);
                for (int k = 0; k < vertsClipped.Count; k++)
                {
                    Vec3[] vertices = new Vec3[]
                    {
                            ProjectVertex(vertsClipped[k][0]),
                            ProjectVertex(vertsClipped[k][1]),
                            ProjectVertex(vertsClipped[k][2])
                    };

                    (Vec3 min, Vec3 max) = GetBoundingBox(vertices);

                    int minX = Math.Max(0, (int)Math.Floor(min.X));
                    int minY = Math.Max(0, (int)Math.Floor(min.Y));
                    int maxX = Math.Min(config.Image_Res[0] - 1, (int)Math.Ceiling(max.X));
                    int maxY = Math.Min(config.Image_Res[1] - 1, (int)Math.Ceiling(max.Y));


                    bool behind = vertices.All(v => v.Z <= 0);

                    bool offScreen = maxX < 0 || minX > config.Image_Res[0] - 1 ||
                    maxY < 0 || minY > config.Image_Res[1] - 1;
                    bool tooFar = vertices.All(v => v.Z > config.Clipping_Range);

                    if (!IsOnScreenT(offScreen, tooFar, behind)) continue;
                    


                    double area = BarycentricCoordinate(vertices[0], vertices[1], vertices[2]);
                    bool backFacing = area < 0;
                    Vec3 c0 = colorsClipped[k][0];
                    Vec3 c1 = colorsClipped[k][1];
                    Vec3 c2 = colorsClipped[k][2];

                    LoopBoundingBox(pixels, minX, minY, maxX, maxY, vertices, area, c0, c1, c2, backFacing, t);

                }
        }
        public void LoopBoundingBox(byte[] pixels, int minX, int minY, int maxX, int maxY, Vec3[] vertices, double area, Vec3 c0, Vec3 c1, Vec3 c2, bool backFacing, Triangle t)
        {
            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {


                    Vec3 p = new Vec3(i + 0.5, j + 0.5, 0);
                    double w0 = BarycentricCoordinate(vertices[1], vertices[2], p);
                    double w1 = BarycentricCoordinate(vertices[2], vertices[0], p);
                    double w2 = BarycentricCoordinate(vertices[0], vertices[1], p);


                    if (area < 0)
                        continue;
                    if (IsInsideTriangle(w0, w1, w2)) ColorPixelTriangle(pixels, t, area, w0, w1, w2, c0, c1, c2, vertices, i, j);


                }
            }
        }
        
        public bool IsOnScreenT(bool offScreen, bool tooFar, bool behind)
        {
            return (!offScreen && !tooFar && !behind);
        }
        public void ColorPixelTriangle(byte[] pixels, Triangle t, double areab, double w0, double w1, double w2, Vec3 c0, Vec3 c1, Vec3 c2, Vec3[] vertices, int i, int j)
        {
            double[] opacity = t.attributes.opacity;

            w0 /= areab;
            w1 /= areab;
            w2 /= areab;
            
            double oneOverZ = w0 / vertices[0].Z + w1 / vertices[1].Z + w2 / vertices[2].Z;
            
            double r = Interpolate(new Vec3(c0.X, c1.X, c2.X), w0, w1,w2, oneOverZ, vertices);
            double g = Interpolate(new Vec3(c0.Y, c1.Y, c2.Y), w0, w1,w2, oneOverZ, vertices);
            double b = Interpolate(new Vec3(c0.Z, c1.Z, c2.Z), w0, w1,w2, oneOverZ, vertices);
            double o = Interpolate(new Vec3(opacity[0], opacity[1], opacity[2]), w0, w1,w2, oneOverZ, vertices);
            

            double z = 1.0 / oneOverZ;
            if (i == config.Image_Res[0] / 2 && j == config.Image_Res[1] / 2 && Globals.timeElapsed > 100 && z < 0)
            {
                int a = 0;
            }
            //if (z < 0) z = -z;

            if (i == config.Image_Res[0] / 2 && j == config.Image_Res[1] / 2) Console.WriteLine($"z: {z} depth: {depths[j * config.Image_Res[0] + i]} 1st vertex: {t.vertices[0].Z}{t.vertices[1].Z}{t.vertices[2].Z} ");
            if (z <= depths[j * config.Image_Res[0] + i])
            {
                
                double intensity = GetTotalLightIntensity(t, t.normal);
                depths[j * config.Image_Res[0] + i] = z;
                int index = (j * config.Image_Res[0] + i) * 4;
                pixels[index + 0] = (byte)(intensity * b * o / 255);
                pixels[index + 1] = (byte)(intensity * g * o / 255);
                pixels[index + 2] = (byte)(intensity *  r * o / 255);
                pixels[index + 3] = (byte)(o);
            }
        }
        public double GetTotalLightIntensity(Object o, Vec3 normal)
        {
            double total = 0;
            foreach(Light light in lights)
            {
                if(o.shading.lambertian)
                {
                    total += light.GetIntensity(normal);
                }
     
            }
            return Math.Clamp(total, 0, 1);
        }
        public double Interpolate(Vec3 attribute, double w0, double w1, double w2, double oneOverZ, Vec3[] vertices)
        {
            return (w0 * attribute.X / vertices[0].Z + w1 * attribute.Y / vertices[1].Z + w2 * attribute.Z / vertices[2].Z) / oneOverZ;
        }
        public bool IsInsideTriangle(double w0, double w1, double w2)
        {
            return (w0 >= 0 && w1 >= 0 && w2 >= 0);
        }
        public double BarycentricCoordinate(Vec3 a, Vec3 b, Vec3 c)
        {
            return  (c.Y - a.Y) * (b.X - a.X) - (c.X - a.X) * (b.Y - a.Y);
        }
        public bool IsBehind(Vec3 vertex)
        {
            return -vertex.Z < 0.1;
        }
        public (Vec3 v, Vec3 c) GetIntersectionPoint(Vec3 a, Vec3 b, Vec3 colorsa, Vec3 colorsb)
        {
            Vec3 v = new Vec3();
            Vec3 c = new Vec3();
            double t = (-0.1 - a.Z) / (b.Z - a.Z);
            v.X = a.X + t * (b.X - a.X);
            v.Y = a.Y + t * (b.Y - a.Y);
            v.Z = -0.1;
            c.X = colorsa.X + t * (colorsb.X - colorsa.X);
            c.Y = colorsa.Y + t * (colorsb.Y - colorsa.Y);
            c.Z = colorsa.Z + t * (colorsb.Z - colorsa.Z);

            return (v, c);
        }
        public (List<Vec3[]>, List<Vec3[]>) ClipVertices(Vec3[] verts, Vec3[] colors)
        {
            bool i0 = IsBehind(verts[0]);
            bool i1 = IsBehind(verts[1]);
            bool i2 = IsBehind(verts[2]);
            int count = (i0 ? 1 : 0) + (i1 ? 1 : 0) + (i2 ? 1 : 0);
            if (count == 3) return (new List<Vec3[]>(), new List<Vec3[]>());

            if (count == 0)
            {
                return (new List<Vec3[]> { verts} , new List<Vec3[]> { colors });
            }
            if (count == 2)
            {
                Vec3 a, b, c;
                Vec3 ca, cb, cc;
                if(!i0)
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
                (ab ,abColor) = GetIntersectionPoint(a, b, ca, cb);
                (ac, acColor) = GetIntersectionPoint(a, c, ca, cc);
                return (new List<Vec3[]>{  new Vec3[] { a, ab, ac } }, new List<Vec3[]> { new Vec3[] { ca, abColor, acColor } });
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
                (ab, abColor) = GetIntersectionPoint(a, b, ca, cb);
                (ac, acColor) = GetIntersectionPoint(a, c, ca, cc);
                return (new List<Vec3[]> { new Vec3[] { ab, b, c }, new Vec3[] { ab, c, ac } }, new List<Vec3[]> { new Vec3[] { abColor, cb, cc }, new Vec3[] { abColor, cc, acColor } });
            }
        }


        public Vec3 ProjectVertex(Vec3 vertex)
        {

            
            
            Matrix4 projectionM = Matrix4.ProjectionMatrix();
            double f = 1 / Math.Tan(config.FOV / 2 / 57.2958D);
            

              
            Vec4 projected = new Vec4(vertex.X, vertex.Y, vertex.Z, 1);
            projected = projectionM * projected;
            double pRasterX = (projected.X + 1) / 2 * config.Image_Res[0];
            double pRasterY = (1 - projected.Y) / 2 * config.Image_Res[1];
            double pRasterZ = -vertex.Z;

            Vec3 v = new Vec3(pRasterX, pRasterY, pRasterZ);



            return v;

        }

    }
}
