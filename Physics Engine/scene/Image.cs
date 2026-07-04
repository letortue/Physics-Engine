
using Aspose.ThreeD.Entities;
using SkiaSharp;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.DataFormats;


namespace Physics_Engine
{
    public class maga
    {
        public Object[] objects;
        public Light[] lights;
        
        
        
        Config config { get; set; }
        Vec3 backgroundColor { get; set; }
        
        public maga(Object[] objects, Light[] lights)
        {
            string json = File.ReadAllText("config.json");
            config = JsonSerializer.Deserialize<Config>(json)!;

            
            
            
            

            backgroundColor = new Vec3(config.BackgroundColor[0], config.BackgroundColor[1], config.BackgroundColor[2]);
            this.objects = objects;
            this.lights = lights;

            
            


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
        public void DrawImage(SKCanvas canvas, SKBitmap map)
        {
            canvas.DrawBitmap(map, 0, 0);
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


        




        public byte[] MapImage(bool IsAntiAlias = true)
        {

            Matrix4 inverse = Globals.Camera.matrix.InverseRotationPart();
            
            
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
            
            SKBitmap map = new SKBitmap(config.Image_res[0], config.Image_res[1], SKColorType.Bgra8888, SKAlphaType.Premul);
            byte[] pixels = new byte[config.Image_res[0] * config.Image_res[1] * 4];


            Parallel.For(0, config.Image_res[1], j =>
            {

            
                for (double i = 0; i < config.Image_res[0]; i++)
                {

                    double sx = startX + i * stepX;
                    double sy = startY + j * stepY;
                    Ray[] rays = new Ray[config.AntiAliasingRes * config.AntiAliasingRes];
                    
                        for (int k = 0; k < config.AntiAliasingRes; k++)
                        {
                            for (int h = 0; h < config.AntiAliasingRes; h++)
                            {
                                double offsetX = (k + 0.5) / config.AntiAliasingRes * stepX;
                                double offsetY = (h + 0.5) / config.AntiAliasingRes * stepY;
                                Vec4 dir4 = new Vec4(sx + offsetX, sy + offsetY, -1, 0);
                                dir4 = Globals.Camera.matrix * dir4;
                                dir4.Normalize();
                                Vec3 dir = new Vec3
                                {
                                    X = dir4.X,
                                    Y = dir4.Y,
                                    Z = dir4.Z
                                };
                                rays[k * config.AntiAliasingRes + h] = new Ray(camWorldPos, dir);

                            }

                        }
                    
                    
                        
                    

                    MapObjects(pixels, rays, i, j);
                }
                
            });

            
            return pixels;

        }
        
        private void MapObjects(byte[] pixels, Ray[] rays, double i, double j)
        {
            int index = (int)((j * config.Image_res[0] + i) * 4);
            Vec3 color = new Vec3(0, 0, 0);

            
            color += CastRay(rays[0], this.objects, 0, out HitResult result, i, j);
            int num = 1;

            for (int k = 1; k < rays.Length; k++)
            {
                color += CastRay(rays[k], this.objects, 0, out HitResult hit, i, j);
                num++;
            }
            color /= num;

            Vec3 minusDir = new Vec3(0 - rays[0].direction.X, 0 - rays[0].direction.Y, 0 - rays[0].direction.Z);
            ColorPixel(pixels, index, color, minusDir, result, result.o);
            
            

        }
        public void ColorPixel(byte[] pixels, int index, Vec3 color, Vec3 minusDir, HitResult result, Object o)
        {
            if (!result.hit)
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

                double facingIntensity;
                facingIntensity = Math.Max(minus, 0);

                pixels[index + 0] = (byte)(facingIntensity * result.color.Z);
                pixels[index + 1] = (byte)(facingIntensity * result.color.Y);
                pixels[index + 2] = (byte)(facingIntensity * result.color.X);
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

        private Vec3 CastRay(Ray R, Object[] objects, int depthRecursion, out HitResult hit, double i,double j)
        {
            hit = new HitResult();
            
            if (depthRecursion > 3) return backgroundColor;

            HitResult result = FindClosestHit(R, objects);
            
            hit = result;
            
            if (!result.hit) return backgroundColor;
            

            if (result.o.shading.isReflective)
            { 
                Vec3 reflectedColor = Reflect(result, R, depthRecursion + 1, i, j);
  
                return result.o.shading.albedo[0] * reflectedColor;

            }
            if(result.o.shading.isRefractive)
            {
                if(result.normal.Dot(R.direction) > 0)
    {
                    result.normal = 0 - result.normal;
                }
                double cosTheta = Math.Abs(result.normal.Dot(0 - R.direction));
                double kr = Schlick(cosTheta, result.o.shading.refIndex);
                Ray refractionRay = RefractRay(R, result);
                
               
                refractionRay.direction = refractionRay.direction.Normalize();
                Vec3 refractedColor = CastRay(refractionRay, this.objects, depthRecursion + 1, out HitResult h, i, j);
                Vec3 reflectedColor = Reflect(result, R, depthRecursion, i, j);
                Vec3 finalColor = (kr *  reflectedColor) +  ((1 - kr) * refractedColor);
                
                
                return result.o.shading.albedo[0] * finalColor;

            }
            return GetSurfaceColor(result.o, result, this.lights, result.albedo, result.o.shading.textureFunc);


        }
        private static Ray RefractRay(Ray R, HitResult result)
        {
            Ray refractionRay = new Ray();
            
            double n = 1.0 / result.o.shading.refIndex;
            double c1 = result.normal.Dot(0-R.direction);
            double c2 = Math.Sqrt(1 - (Math.Pow(n, 2) * (1 - Math.Pow(c1, 2))));
            refractionRay.direction = (n * R.direction + (((n * c1) - c2) * result.normal)).Normalize();
           
            if (result.o is Ball ball)
            {
                double exitT = FindExitT(ball.vertices[0], result.point, refractionRay.direction, ball.radius);
                if (exitT < 0)
                {
                    refractionRay.direction = (R.direction - (2 * (result.normal.Dot(R.direction)) * result.normal)).Normalize();
                    refractionRay.origin = result.point + 1e-6 * result.normal;
                    return refractionRay;
                }
                Vec3 exitPoint = result.point + exitT * refractionRay.direction;
                Vec3 exitNormal = 0-(exitPoint - ball.vertices[0]).Normalize();

                
                double n2 = result.o.shading.refIndex / 1.0; 
                double c1exit = exitNormal.Dot(0-refractionRay.direction);
                double c2exit = Math.Sqrt(1 - (Math.Pow(n2, 2) * (1 - Math.Pow(c1exit, 2))));

                refractionRay.direction = (n2 * refractionRay.direction + (((n2 * c1exit) - c2exit) * exitNormal)).Normalize();
                refractionRay.origin = exitPoint + 1e-6 * (0-exitNormal);

            }
            else
            {
                refractionRay.origin = result.point - (1e-06 * result.normal);
            }
            
            
            return refractionRay;
            
        }
        private Vec3 Reflect(HitResult result, Ray R, int depthRecursion, double i, double j)
        {
            if (result.o.shading.oneSided && result.normal.Dot(R.direction) > 0)
            {
                return GetSurfaceColor(result.o, result, this.lights, result.albedo, result.o.shading.textureFunc);
            }
            else if (result.normal.Dot(R.direction) > 0)
            {
                result.normal = 0 - result.normal;
            }


            Ray reflectionRay = new Ray
            {
                direction =( R.direction - (2 * (result.normal.Dot(R.direction)) * result.normal)).Normalize(),
                origin = result.point + (1e-06 * result.normal)
            };

            Vec3 reflectedColor = CastRay(reflectionRay, this.objects, depthRecursion + 1, out HitResult h,i ,j);
            return reflectedColor;
        }
        double Schlick(double cosTheta, double ior)
        {
            double r0 = Math.Pow((1 - ior) / (1 + ior), 2);
            return r0 + (1 - r0) * Math.Pow(1 - cosTheta, 5);
        }
        private static double FindExitT(Vec3 sphereCenter, Vec3 P, Vec3 D, double radius)
        {
            Vec3 L = P - sphereCenter;
            double b = 2 * D.Dot(L);
            double c = L.Dot(L) - radius * radius;
            double discriminant = b * b - 4 * c;
            if (discriminant < 0)
            {
                // total internal reflection - treat as mirror
                // this is what's likely happening at steep angles
                if (discriminant < 0) return -1;
            }
            double t1 = (-b - Math.Sqrt(discriminant)) / 2;
            double t2 = (-b + Math.Sqrt(discriminant)) / 2;
            if (t1 > t2) return t1;
            return t2;
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
        
        private bool IsLit(Vec3 minusDir, HitResult hit, Light light)
        {
            
                if (light is SpotLight spot)
                {
                    if (spot.falloff > spot.GetDirection(hit.point).Normalize().Dot(spot.facingDir)) return false; 
                    
                }
                foreach (Object ob in objects)
                {
                    if (IsInShadow(minusDir, hit, ob, light)) { return false; }
                    
                }
                return true;
        }
        
        public Vec3 GetSurfaceColor(Object o, HitResult hit, Light[] lights, Vec3 albedo,  Func<Vec3, Vec3> textureFunc)
        {
            albedo = albedo / Math.PI;
            //physics accurate normalization
            Vec3 contributions = new Vec3(0, 0, 0);
            foreach (Light light in lights)
            {
                Vec3 dir = light.GetDirection(hit.point);
                Vec3 minusDir = new Vec3(0 - dir.X, 0 - dir.Y, 0 - dir.Z);

                if (IsLit( minusDir, hit, light))
                {
                    double dot;
                    if (o is Disk) dot = Math.Abs(minusDir.Dot(hit.normal));
                    else dot = minusDir.Dot(hit.normal);

                    
                    contributions += Math.Max(dot, 0) * light.GetIntensity(hit.point) * light.color  ;
                }

            }
            

            return albedo * contributions;

        }
                  
        
        

    }

}
