using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public Vec3[] color { get; set; }
        public double[] opacity { get; set; }
    }










    public class Ball : Object
    {


        public Ball(Vec3 coords, Vec3 v, Vec3 a)
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


        }



    }
    public class PolyHedron : Object
    {




        public PolyHedron(int npoints, Vec3[] coords, Vec3 v, Vec3 a)
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



        }



    }
    public class Triangle : Object
    {
        
        public Triangle(Vec3[] coords, Vec3[] v, Vec3[] a, Vec3[] color, double[] opacity)
        {
            this.color = new Vec3[3];
            this.coordinates = new Vec3[3];
            this.velocity = new Vec3[3];
            this.acceleration = new Vec3[3];
            this.opacity = opacity;
            for (int i = 0; i < 3; i++)
            {
                this.color[i] = color[i];
                this.coordinates[i].X = coords[i].X;
                this.velocity[i].X = v[i].X;
                this.acceleration[i].X = a[i].X;
                this.coordinates[i].Y = coords[i].Y;
                this.velocity[i].Y = v[i].Y;
                this.acceleration[i].Y = a[i].Y;
                this.coordinates[i].Z = coords[i].Z;
                this.velocity[i].Z = v[i].Z;
                this.acceleration[i].Z = a[i].Z;
            }

        }
    }

}