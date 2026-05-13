using Aspose.ThreeD.Entities;
using SkiaSharp;
using System;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Security.AccessControl;
using System.Text.Json;
using static System.Windows.Forms.Design.AxImporter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

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
        Vec3 backgroundColor { get; set; }
        Matrix4 ObjectToWorldMatrix { get; set; }
        public Image(Object[] objects, Light[] lights)
        {
            string json = File.ReadAllText("config.json");
            config = JsonSerializer.Deserialize<Config>(json)!;

            bitmap = new SKBitmap(config.Image_res[0], config.Image_res[1], SKColorType.Bgra8888, SKAlphaType.Premul);
            this.pixels = new byte[config.Image_res[0] * config.Image_res[1] * 4];
            this.depths = new double[config.Image_res[0] * config.Image_res[1]];
            Array.Fill(depths, double.PositiveInfinity);

            backgroundColor = new Vec3(config.BackgroundColor[0], config.BackgroundColor[1], config.BackgroundColor[2]);
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
            int index = (int)((j * config.Image_res[0] + i) * 4);
            Vec3 minusDir = new Vec3(0 - R.direction.X, 0 - R.direction.Y, 0 - R.direction.Z);
            minusDir = minusDir.Normalize();

            Vec3 color = CastRay(R, this.objects, 0, out HitResult result);
            
            ColorPixel(index, color, minusDir, result, result.o);
            
            

        }
        public bool IsVisible(HitResult result, double depth, double i, double j)
        {
            if (!result.hit || result.t <= 0 || result.t > config.Clipping_range || depth > depths[(int)(j * config.Image_res[0] + i)]) return false;
            return true;
        }

        private Vec3 CastRay(Ray R, Object[] objects, int depthRecursion, out HitResult hit)
        {
            hit = new HitResult();
            if (depthRecursion > 3) return backgroundColor;

            HitResult result = FindClosestHit(R, objects);
            hit = result;
            if (!result.hit) return backgroundColor;
            

            if (result.o.shading.isReflective)
            {

                Ray reflectionRay = new Ray
                {
                    direction = R.direction - (2 * (result.normal.Dot(R.direction)) * result.normal),
                    origin = result.point + (1e-06 * result.normal)
                };
                Vec3 reflectedColor = CastRay(reflectionRay, this.objects, depthRecursion + 1, out HitResult h);
                return result.o.shading.albedo[0] * reflectedColor;

            }

            return GetSurfaceColor(result.o, result, this.lights, result.o.shading.albedo[0]);


        }

        private HitResult FindClosestHit(Ray R, Object[] objects)
        {

            HitResult resultHit = new HitResult();
            double depth = double.PositiveInfinity;
            foreach (Object o in objects)
            {
                HitResult r;
                if (o is Mesh mesh)
                {
                    r = FindClosestHit(R, mesh.triangles);
                    if (r.hit && r.t < depth) { depth = r.t; resultHit = r; }
                    continue;
                }
                
                r = o.GetIntersectionPoint(R);
                 

                if (r.t <= 0 || !r.hit || r.t > config.Clipping_range) continue;
                if (r.t < depth)
                {
                    depth = r.t;
                    resultHit = r;
                    
                }


            }
            return resultHit;
        }

        public void ColorPixel(int index, Vec3 color, Vec3 minusDir, HitResult result, Object o)
        {
            if(!result.hit)
            {
                byte colorX = (byte)Math.Clamp(color.X, 0, 255);
                byte colorY = (byte)Math.Clamp(color.Y, 0, 255);
                byte colorZ = (byte)Math.Clamp(color.Z, 0, 255);
                pixels[index + 0] = (byte)(colorZ);
                pixels[index + 1] = (byte)(colorY);
                pixels[index + 2] = (byte)(colorX);
                pixels[index + 3] = 255;
                return;
            }

            if (o.shading.facing_ratio[0])
            {
                double minus = minusDir.Dot(result.normal);

                double facingIntesity;
                facingIntesity = Math.Max(minus, 0);

                pixels[index + 0] = (byte)(facingIntesity * result.color.Z);
                pixels[index + 1] = (byte)(facingIntesity * result.color.Y);
                pixels[index + 2] = (byte)(facingIntesity * result.color.X);
                pixels[index + 3] = 255;
            }
            else
            {
                
                
                byte colorX = (byte)Math.Clamp(color.X, 0, 255);
                byte colorY = (byte)Math.Clamp(color.Y, 0, 255);
                byte colorZ = (byte)Math.Clamp(color.Z, 0, 255);
                pixels[index + 0] = (byte)(colorZ);
                pixels[index + 1] = (byte)(colorY);
                pixels[index + 2] = (byte)(colorX);
                pixels[index + 3] = 255;
            }
        }
        
        
        public bool HandleShadow(Object o, HitResult hit, Vec3 minusDir, Light light)
        {
            HitResult result = o.GetIntersectionPoint(new Ray(hit.point + (1e-4 * hit.normal), minusDir));
            if (light is not PointLight)
            {
                if (result.hit && result.t > 0 && result.t < config.Clipping_range) { return true; }
            }
            else
            {
                PointLight pointLight = (PointLight)light;
                Vec3 distanceLight = hit.point - pointLight.pos;
                double lightDist = (pointLight.pos - hit.point).Magnitude();
                if (result.hit && result.t > 0 && result.t < lightDist) { return true; }
            }
            return false;
        }
        
        public bool IsInShadow(Vec3 minusDir, HitResult hit, Object o, Light light)
        {
            
            if(o is Mesh)
            {
                
                Mesh mesh = (Mesh)o;
                
                foreach(Triangle t in mesh.triangles)
                {
                    if (HandleShadow(t, hit, minusDir, light)) return true;
                   
                }
            }
            else
            {
                if (HandleShadow(o, hit, minusDir, light)) return true;
            }
            return false;
        }
        

        
        public Vec3 GetSurfaceColor(Object o, HitResult hit, Light[] lights, Vec3 albedo)
        {
            albedo = albedo / Math.PI;
            Vec3 contributions = new Vec3(0, 0, 0);
            foreach (Light light in lights)
            {
                Vec3 dir = light.GetDirection(hit.point);
                Vec3 minusDir = new Vec3(0 - dir.X, 0 - dir.Y, 0 - dir.Z);
                bool isInShadow = false;
                foreach (Object ob in objects)
                {
                    if (IsInShadow(minusDir, hit, ob, light)) { isInShadow = true; break; }
                }
                if (!isInShadow)
                {
                    double dot;
                    if (o is Disk) dot = Math.Abs(minusDir.Dot(hit.normal));
                    else dot = minusDir.Dot(hit.normal);
                    contributions += Math.Max(dot, 0) * light.GetIntensity(hit.point) * light.color;
                }

            }
            

            return albedo * contributions;

        }
                  
        


    }
}
