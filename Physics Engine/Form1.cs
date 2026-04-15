namespace Physics_Engine
{
    using Accessibility;
    using OpenTK.Graphics.OpenGL;
    using SkiaSharp;
    using SkiaSharp.Views.Desktop;
    using System.Collections.Generic;
    using System.Drawing.Text;
    using System.Runtime.InteropServices.Marshalling;
    using System.Security.Policy;
    using System.Text.Json;
    using System.Windows.Forms;


    public partial class Form1 : Form
    {

        
        private SKControl skControl;
        public float timeElapsed;

        //private Ball ball1;
        //private Ball ball2;
        private PolyHedron p;




        public Form1()
        {



            //

            string json = File.ReadAllText("config.json");
            Config config = JsonSerializer.Deserialize<Config>(json)!;


            InitializeComponent();
         
            this.ClientSize = new Size(config.window_res[0], config.window_res[1]);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            skControl = new SKControl();
            skControl.Size = new Size(config.window_res[0], config.window_res[1]);
            skControl.Dock = DockStyle.Fill;
            skControl.PaintSurface += OnPaintSurface;

            Controls.Add(skControl);
            //


            var paint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true,
                StrokeWidth = 5

            };
            var paint2 = new SKPaint
            {
                Color = SKColors.Blue,

            };


            Vec3[] velo = [new Vec3(0, 0.0, 0.0), new Vec3(-2, -0.5, 0)];
            Vec3[] acc = [new Vec3(-0.00, -0.00, 0.0), new Vec3(-1, -9.81, -2)];
            Vec3[] coordss =
            [
                new Vec3(-1, -1, -10),
                new Vec3(-1, -1, -3),
                new Vec3(-1,  1, -10),
                new Vec3(-1,  1, -3),
                new Vec3(1, -1,  -10),
                new Vec3(1, -1,  -3),
                new Vec3(1,  1,  -10),
                new Vec3(1,  1,  -3),

            ];

            Matrix4 m1 = new Matrix4();
            Matrix4 m2 = new Matrix4();
            m1.data = new double[4,4] { 
                { 1, 3, 1, 0 },
                { 6, 4, 7, 0 },
                { 1, 2, 0, 0 },
                { 0, 0, 0, 0 } 
            };
            m2.data = new double[4,4] { 
                { 1, 3, 1, 0 },
                { 6, 4, 7, 0 },
                { 1, 2, 0, 0 },
                { 0, 0, 0, 0 } 
            };
            

            p = new PolyHedron(8, coordss, velo[0], acc[0], paint);
            

            //
            int pixelsPerMeter = 10;
            Timer timer = new Timer();
            timeElapsed = 0;
            timer.Interval = config.interval;
            timer.Tick += (s, e) =>
            {
                Globals.Camera.move(new Vec3(0,0,-0.05));
                if(timeElapsed == 100)
                {
                    Globals.Camera.rotate(0,Math.PI/2);
                }
                
                p.update(p, pixelsPerMeter);
                Console.WriteLine(Globals.Camera.matrix);

                timeElapsed++;
                
                this.Text = $"{timeElapsed}, {timeElapsed / 60}";


                skControl.Invalidate();
            };
            timer.Start();
            //
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {

            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            
            //ball1.draw(canvas);
            //ball2.draw(canvas);
            p.draw(canvas, 1 );
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
    
}
