using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Physics_Engine
{
    public abstract class Object
    {
        public Vec3[] coordinates { get; set; }
        public Vec3[] velocity { get; set; }
        public Vec3[] acceleration { get; set; }
        public SKPaint paint { get; set; }
        
        





        public void draw(SKCanvas canvas, int radius = 2)
        {
            string json = File.ReadAllText("config.json");
            Config config = JsonSerializer.Deserialize<Config>(json)!;

            for (int i = 0; i < this.coordinates.Length; i++)
            {
                
                Vec3 transformed = new Vec3(this.coordinates[i].X, this.coordinates[i].Y, this.coordinates[i].Z);
                Matrix4 inverse = Globals.Camera.matrix.InverseRotationPart();
                transformed = transformed.WorldToCamera(inverse);
                
                double x = transformed.X;
                double y = transformed.Y;
                double z = transformed.Z;
                //Console.WriteLine($"{x}, {y}, {z}");
                if (transformed.dot(new Vec3(0,0,-1)) > 0)
                {

                    double pScreenX = -x / z;
                    double pScreenY = y / z;
                    double pNDCX = ((pScreenX + config.canvas_res[0]) / 2) / config.canvas_res[0];
                    double pNDCY = ((pScreenY + config.canvas_res[1]) / 2) / config.canvas_res[1];
                    double pRasterX = pNDCX * config.window_res[0];
                    double pRasterY = pNDCY * config.window_res[1];
                    Console.WriteLine($"{pRasterX}, {pRasterY}");
                    canvas.DrawPoint((float)pRasterX, (float)pRasterY, paint);
                    //Console.WriteLine($"{x},{y}");

                }


                
            }
 


        }




        public void update(Object o, double pixelsPerMeter = 50)
        {
            string json = File.ReadAllText("config.json");
            Config config = JsonSerializer.Deserialize<Config>(json)!;
            double t = (float) config.interval / 1000 ;
            int v = velocity.Length - 1;
            for (int j = 0; j < o.coordinates.Length; j++)
            {
                
                    o.velocity[0].X += t * o.acceleration[0].X;
                    o.coordinates[j].X += t * o.velocity[0].X * pixelsPerMeter;
                    o.velocity[0].Y += t * o.acceleration[0].Y;
                    o.coordinates[j].Y += t * o.velocity[0].Y * pixelsPerMeter;
                    o.velocity[0].Z += t * o.acceleration[0].Z;
                    o.coordinates[j].Z += t * o.velocity[0].Z * pixelsPerMeter;
                
            }
        }


    }
    public class Ball : Object
    {

        
        public Ball(Vec3 coords, Vec3 v, Vec3 a, SKPaint paint)
        {
            this.coordinates = new Vec3[1];
            this.velocity = new Vec3[1];
            this.acceleration = new Vec3[1];
            
            this.coordinates[0].X = coords.X;
            this.velocity[0].X = v.X;
            this.acceleration[0].X = a.X;
            this.coordinates[0].Y = coords.Y;
            this.velocity[0].Y = v.Y;
            this.acceleration[0].Y = a.Y;
            this.coordinates[0].Z = coords.Z;
            this.velocity[0].Z = v.Z;
            this.acceleration[0].Z = a.Z;
            

            this.paint = paint;
        }

        

    }
    public class PolyHedron : Object
    {
        

        

        public PolyHedron(int npoints, Vec3[] coords, Vec3 v, Vec3 a, SKPaint paint)
        {
            this.coordinates = new Vec3[npoints];
            this.velocity = new Vec3[1];
            this.acceleration = new Vec3[1];
            for (int i = 0; i < npoints; i++)
            {
                
                    this.coordinates[i].X = coords[i].X;
                    this.velocity[0].X = v.X;
                    this.acceleration[0].X = a.X;
                    this.coordinates[i].Y = coords[i].Y;
                    this.velocity[0].Y = v.Y;
                    this.acceleration[0].Y = a.Y;
                    this.coordinates[i].Z = coords[i].Z;
                    this.velocity[0].Z = v.Z;
                    this.acceleration[0].Z = a.Z;

            }
            

            this.paint = paint;
        }

        

    }
}
