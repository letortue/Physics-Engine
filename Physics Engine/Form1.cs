namespace Physics_Engine
{
    using System.Collections.Generic;
    using OpenTK.Graphics.OpenGL;
    using SkiaSharp;
    using SkiaSharp.Views.Desktop;
    using System.Drawing.Text;
    using System.Runtime.InteropServices.Marshalling;
    using System.Windows.Forms;
    using Accessibility;

    public partial class Form1 : Form
    {
        private SKControl skControl;

        //private Ball ball1;
        //private Ball ball2;
        private PolyHedron p;


        public Form1()
        {
            InitializeComponent();

            this.ClientSize = new Size(500, 500);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            skControl = new SKControl();
            skControl.Size = new Size(500, 500);
            skControl.Dock = DockStyle.Fill;
            skControl.PaintSurface += OnPaintSurface;

            Controls.Add(skControl);



            var paint = new SKPaint
            {
                Color = SKColors.Red

            };
            var paint2 = new SKPaint
            {
                Color = SKColors.Blue

            };

            Vec3[] coords = [new Vec3(0, 0, -10), new Vec3(300, 100, -20)];
            Vec3[] velo = [new Vec3(0, 0, 0), new Vec3(-2, -0.5, 0)];
            Vec3[] acc = [new Vec3(-0.003, -0.003, -.01), new Vec3(-1, -9.81, -2)];
            Vec3[] coordss =
            [
                new Vec3(-1, -1, -20),
                new Vec3(-1, -1, -3),
                new Vec3(-1,  1, -20),
                new Vec3(-1,  1, -3),
                new Vec3(1, -1,  -20),
                new Vec3(1, -1,  -3),
                new Vec3(1,  1,  -20),
                new Vec3(1,  1,  -3),
                
            ];

            //ball1 = new Ball(coords[0], velo[0], acc[0], paint);
            //ball2 = new Ball(coords[1], velo[1], acc[1], paint);
            p = new PolyHedron(8, coordss, velo[0], acc[0], paint);

            int pixelsPerMeter = 10;
            Timer timer = new Timer();
            float timeElapsed = 0;
            timer.Interval = 16;
            timer.Tick += (s, e) =>
            {
                //ball1.update(ball1, pixelsPerMeter);
                //ball2.update(ball1, pixelsPerMeter);
                p.update(p, pixelsPerMeter);

                timeElapsed++;
                //this.Text = $"{timeElapsed}, {timeElapsed / 60}";
                Vec3 v1 = new Vec3(1, 7, 3);
                Vec3 v2 = new Vec3(3, 2, 1);
                this.Text = $"{(v1.dot(v2))}";

                skControl.Invalidate();
            };
            timer.Start();
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            
            //ball1.draw(canvas);
            //ball2.draw(canvas);
            p.draw(canvas,2, 1, [500, 500] );

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
    
}
