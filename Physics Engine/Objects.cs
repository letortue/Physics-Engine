using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Engine
{
    public abstract class Object
    {
        public Vec3[] coordinates { get; set; }
        public Vec3[] velocity { get; set; }
        public Vec3[] acceleration { get; set; }
        public SKPaint paint { get; set; }


        public void draw(SKCanvas canvas, int upper_bound, double aspect_ratio, int[] size, int radius = 2)
        {
            for (int i = 0; i < this.coordinates.Length; i++)
            {
                double x = ((1 + (-this.coordinates[i].X / this.coordinates[i].Z)) / 2) * size[0] / aspect_ratio;
                double y = ((1 + (-this.coordinates[i].Y / this.coordinates[i].Z)) / 2) * size[1];

                canvas.DrawCircle((float)x, (float)y, radius, paint);
            }




        }


        public void update(Object o, double pixelsPerMeter = 50)
        {
            double t = 0.016;
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
