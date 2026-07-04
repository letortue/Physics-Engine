using Aspose.ThreeD;
using Aspose.ThreeD.Formats;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.DataFormats;

namespace Physics_Engine.Engine
{
    public class RayTracer : IRenderEngine
    {
        Camera camera;
        public RayTracer(Camera camera) 
        {
            this.camera = camera;
        }
        public byte[] RenderImage(Scene scene)
        {
            Config config = scene.config;

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





                    MapObjects(scene, config, pixels, rays, i, j);
                }

            });


            return pixels;

        }

        private void MapObjects(Scene scene, Config config, byte[] pixels, Ray[] rays, double i, double j)
        {
            int index = (int)((j * config.Image_res[0] + i) * 4);
            Vec3 color = new Vec3(0, 0, 0);


            color += CastRay(scene, config, rays[0], scene.objects, 0, out HitResult result, i, j);
            int num = 1;

            for (int k = 1; k < rays.Length; k++)
            {
                color += CastRay(scene, config, rays[k], scene.objects, 0, out HitResult hit, i, j);
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

        private Vec3 CastRay(Scene scene, Config config, Ray R, Object[] objects, int depthRecursion, out HitResult hit, double i, double j)
        {
            hit = new HitResult();
            Vec3 backgroundColor = new Vec3(config.BackgroundColor[0], config.BackgroundColor[1], config.BackgroundColor[2]);

            if (depthRecursion > 3) return backgroundColor;

            HitResult result = FindClosestHit(scene, config, R, objects);

            hit = result;

            if (!result.hit) return backgroundColor;


            if (result.o.shading.isReflective)
            {
                Vec3 reflectedColor = Reflect(scene, config, result, R, depthRecursion + 1, i, j);

                return result.o.shading.albedo[0] * reflectedColor;

            }
            if (result.o.shading.isRefractive)
            {
                if (result.normal.Dot(R.direction) > 0)
                {
                    result.normal = 0 - result.normal;
                }
                double cosTheta = Math.Abs(result.normal.Dot(0 - R.direction));
                double kr = Schlick(cosTheta, result.o.shading.refIndex);
                Ray refractionRay = RefractRay(R, result);


                refractionRay.direction = refractionRay.direction.Normalize();
                Vec3 refractedColor = CastRay(scene, config, refractionRay, scene.objects, depthRecursion + 1, out HitResult h, i, j);
                Vec3 reflectedColor = Reflect(scene, config, result, R, depthRecursion, i, j);
                Vec3 finalColor = (kr * reflectedColor) + ((1 - kr) * refractedColor);


                return result.o.shading.albedo[0] * finalColor;

            }
            return GetSurfaceColor(scene, config, result.o, result, scene.lights, result.albedo, result.o.shading.textureFunc);


        }
        private static Ray RefractRay(Ray R, HitResult result)
        {
            Ray refractionRay = new Ray();

            double n = 1.0 / result.o.shading.refIndex;
            double c1 = result.normal.Dot(0 - R.direction);
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
                Vec3 exitNormal = 0 - (exitPoint - ball.vertices[0]).Normalize();


                double n2 = result.o.shading.refIndex / 1.0;
                double c1exit = exitNormal.Dot(0 - refractionRay.direction);
                double c2exit = Math.Sqrt(1 - (Math.Pow(n2, 2) * (1 - Math.Pow(c1exit, 2))));

                refractionRay.direction = (n2 * refractionRay.direction + (((n2 * c1exit) - c2exit) * exitNormal)).Normalize();
                refractionRay.origin = exitPoint + 1e-6 * (0 - exitNormal);

            }
            else
            {
                refractionRay.origin = result.point - (1e-06 * result.normal);
            }


            return refractionRay;

        }
        private Vec3 Reflect(Scene scene, Config config, HitResult result, Ray R, int depthRecursion, double i, double j)
        {
            if (result.o.shading.oneSided && result.normal.Dot(R.direction) > 0)
            {
                return GetSurfaceColor(scene, config, result.o, result, scene.lights, result.albedo, result.o.shading.textureFunc);
            }
            else if (result.normal.Dot(R.direction) > 0)
            {
                result.normal = 0 - result.normal;
            }


            Ray reflectionRay = new Ray
            {
                direction = (R.direction - (2 * (result.normal.Dot(R.direction)) * result.normal)).Normalize(),
                origin = result.point + (1e-06 * result.normal)
            };

            Vec3 reflectedColor = CastRay(scene, config, reflectionRay, scene.objects, depthRecursion + 1, out HitResult h, i, j);
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
        private HitResult FindClosestHit(Scene scene, Config config, Ray R, Object[] objects)
        {


            HitResult resultHit = new HitResult();
            double depth = double.PositiveInfinity;
            foreach (Object o in objects)
            {
                HitResult r;
                if (o is Mesh mesh)
                {

                    r = FindClosestHit(scene, config, R, mesh.triangles);
                    if (r.hit && r.t < depth) { depth = r.t; resultHit = r; }
                    continue;
                }

                r = GetIntersectionPoint(o, R);



                if (r.t <= 0 || !r.hit || r.t > config.Clipping_range) continue;
                if (r.t < depth)
                {
                    depth = r.t;
                    resultHit = r;

                }


            }
            return resultHit;
        }




        public bool HandleShadow(Scene scene, Config config, Object o, HitResult hit, Vec3 minusDir, Light light)
        {
            HitResult result = GetIntersectionPoint(o,new Ray(hit.point + (1e-4 * hit.normal), minusDir));
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

        public bool IsInShadow(Scene scene, Config config, Vec3 minusDir, HitResult hit, Object o, Light light)
        {

            if (o is Mesh)
            {

                Mesh mesh = (Mesh)o;

                foreach (Triangle t in mesh.triangles)
                {
                    if (HandleShadow(scene, config, t, hit, minusDir, light)) return true;

                }
            }
            else
            {
                if (HandleShadow(scene, config, o, hit, minusDir, light)) return true;
            }
            return false;
        }

        private bool IsLit(Scene scene, Config config, Vec3 minusDir, HitResult hit, Light light)
        {

            if (light is SpotLight spot)
            {
                if (spot.falloff > spot.GetDirection(hit.point).Normalize().Dot(spot.facingDir)) return false;

            }
            foreach (Object ob in scene.objects)
            {
                if (IsInShadow(scene, config, minusDir, hit, ob, light)) { return false; }

            }
            return true;
        }

        public Vec3 GetSurfaceColor(Scene scene, Config config, Object o, HitResult hit, Light[] lights, Vec3 albedo, Func<Vec3, Vec3> textureFunc)
        {
            albedo = albedo / Math.PI;
            //physics accurate normalization
            Vec3 contributions = new Vec3(0, 0, 0);
            foreach (Light light in lights)
            {
                Vec3 dir = light.GetDirection(hit.point);
                Vec3 minusDir = new Vec3(0 - dir.X, 0 - dir.Y, 0 - dir.Z);

                if (IsLit(scene, config, minusDir, hit, light))
                {
                    double dot;
                    if (o is Disk) dot = Math.Abs(minusDir.Dot(hit.normal));
                    else dot = minusDir.Dot(hit.normal);


                    contributions += Math.Max(dot, 0) * light.GetIntensity(hit.point) * light.color;
                }

            }


            return albedo * contributions;

        }
        public HitResult GetIntersectionPoint(Object o, Ray r)
        {
           

            if (o is Mesh mesh) return GetIntersectionPointMesh(mesh, r);
            if (o is Ball ball) return GetIntersectionPointBall(ball, r);
            if (o is Plane plane) return GetIntersectionPointPlane(plane, r);
            if (o is Disk disk) return GetIntersectionPointDisk(disk, r);
            else return new HitResult();

        }

        public HitResult GetIntersectionPointMesh(Mesh mesh, Ray r)
        {
            HitResult closest = new HitResult { hit = false };
            foreach (Triangle tri in mesh.triangles)
            {
                HitResult result = GetIntersectionPointTriangle(tri, r);
                if (result.hit && (!closest.hit || result.t < closest.t))
                    closest = result;
            }
            return closest;
            // onesided bug stems from the fact that some faces' vertices are defined in the wrong order -
            // the order has to be clockwise according to the original coordinate system
        }

        public HitResult GetIntersectionPointBall(Ball ball, Ray r)
        {

            Vec3 L = r.origin - ball.vertices[0];
            Vec3 dir = r.direction;
            HitResult result = new HitResult();
            result.color = ball.attributes.colors[0];

            result.o = ball;
            bool intersects = Maths.SolveQuadratic(dir.Dot(dir), 2 * L.Dot(dir), L.Dot(L) - Math.Pow(ball.radius, 2), out double? t1, out double? t2);
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
                    result.albedo = ball.shading.textureFunc is not null ? ball.shading.textureFunc(result.point) : ball.shading.albedo[0];
                    result.normal = (result.point - ball.vertices[0]).Normalize();
                    result.t = (double)t1;
                    return result;
                }
                else
                {
                    if (t1 < t2)
                    {
                        result.point = r.origin + (double)t1 * dir;
                        result.albedo = ball.shading.textureFunc is not null ? ball.shading.textureFunc(result.point) : ball.shading.albedo[0];
                        result.normal = (result.point - ball.vertices[0]).Normalize();
                        result.t = (double)t1;
                    }
                    else
                    {
                        result.point = r.origin + (double)t2 * dir;
                        result.albedo = ball.shading.textureFunc is not null ? ball.shading.textureFunc(result.point) : ball.shading.albedo[0];
                        result.normal = (result.point - ball.vertices[0]).Normalize();
                        result.t = (double)t2;
                    }

                    return result;
                }


            }
        }
        public HitResult GetIntersectionPointPlane(Plane plane, Ray r)
        {
            HitResult result = new HitResult();
            double denom = r.direction.Dot(plane.normal);
            if (Math.Abs(denom) < 1e-6) { result.hit = false; return result; }
            result.t = (plane.point - r.origin).Dot(plane.normal) / denom;
            if (result.t < 0) { result.hit = false; return result; }
            result.normal = plane.normal;
            result.o = plane;
            result.hit = true;
            result.point = r.origin + (result.t * r.direction);
            result.color = plane.attributes.colors[0];
            result.albedo = plane.shading.textureFunc is not null ? plane.shading.textureFunc(result.point) : plane.shading.albedo[0];


            return result;

        }
        public HitResult GetIntersectionPointDisk(Disk disk, Ray r)
        {


            HitResult result = GetIntersectionPointPlane(disk.p, r);
            double denom = r.direction.Dot(disk.normal);
            if (Math.Abs(denom) < 1e-6) { result.hit = false; return result; }
            result.albedo = disk.shading.textureFunc is not null ? disk.shading.textureFunc(result.point) : disk.shading.albedo[0];
            result.color = disk.attributes.colors[0];
            result.normal = disk.normal;
            result.o = disk;
            result.textureT = result.point.X;
            result.textureV = result.point.Y;
            if (!result.hit)
            {
                return result;
            }

            Vec3 v = result.point - disk.vertices[0];
            double d2 = v.Dot(v);

            if (d2 <= disk.square_radius)
            {

                result.t = (disk.vertices[0] - r.origin).Dot(disk.normal) / denom;
                if (result.t < 0) { result.hit = false; return result; }
                result.normal = disk.normal;
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
        public HitResult GetIntersectionPointTriangle(Triangle triangle, Ray r)
        {
            Vec3 tuv = Maths.CramersRule(r, triangle.vertices);
            double w0 = 1 - tuv[1] - tuv[2];
            double w1 = tuv[1];
            double w2 = tuv[2];
            if (w0 < 0 || w1 < 0 || w2 < 0 || w0 > 1 || w1 > 1 || w2 > 1) { return new HitResult { hit = false }; }

            if ((triangle.onesided && triangle.normal.Dot(r.direction) > 0) || Math.Abs(triangle.normal.Dot(r.direction)) < 1e-6 || tuv[0] < 0) return new HitResult { hit = false };

            Vec3 point = r.origin + tuv[0] * r.direction;
            HitResult result = new HitResult
            {
                hit = true,
                point = point,
                normal = triangle.normal,
                t = tuv[0],
                w0 = w0,
                w1 = w1,
                w2 = w2,
                o = triangle
            };
            Vec3 offsetNormal = r.direction.Dot(triangle.normal) < 0 ? triangle.normal : 0 - triangle.normal;
            result.point = point + 1e-4 * offsetNormal;
            result.normal = offsetNormal;
            double R = triangle.attributes.colors[0].X * w0 + triangle.attributes.colors[1].X * w1 + triangle.attributes.colors[2].X * w2;
            double G = triangle.attributes.colors[0].Y * w0 + triangle.attributes.colors[1].Y * w1 + triangle.attributes.colors[2].Y * w2;
            double B = triangle.attributes.colors[0].Z * w0 + triangle.attributes.colors[1].Z * w1 + triangle.attributes.colors[2].Z * w2;
            result.color.X = R; result.color.Y = G; result.color.Z = B;

            if (triangle.shading.textureFunc is not null)
            {
                result.textureT = triangle.attributes.textureT[0] * w0 + triangle.attributes.textureT[1] * w1 + triangle.attributes.textureT[2] * w2;
                result.textureV = triangle.attributes.textureV[0] * w0 + triangle.attributes.textureV[1] * w1 + triangle.attributes.textureV[2] * w2;
                result.albedo = triangle.shading.textureFunc(result.point);
                return result;
            }


            if (triangle.shading.isInterpolatedAlbedo)
            {
                double albedoR = triangle.attributes.albedo[0].X * w0 + triangle.attributes.albedo[1].X * w1 + triangle.attributes.albedo[2].X * w2;
                double albedoG = triangle.attributes.albedo[0].Y * w0 + triangle.attributes.albedo[1].Y * w1 + triangle.attributes.albedo[2].Y * w2;
                double albedoB = triangle.attributes.albedo[0].Z * w0 + triangle.attributes.albedo[1].Z * w1 + triangle.attributes.albedo[2].Z * w2;
                result.albedo.X = albedoR;
                result.albedo.Y = albedoG;
                result.albedo.Z = albedoB;
            }
            else
                result.albedo = triangle.shading.albedo[0];





            return result;





        }
    }

}

