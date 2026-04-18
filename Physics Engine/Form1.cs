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
        Config config { get; set; }
        
        private SKControl skControl;
        private bool[] KeyPressed;



        //private Ball ball1;
        //private Ball ball2;
        private Triangle t1;
        private Triangle t2;
        public SKCanvas canvas;
        Image image;


        public Form1()
        {



            //
            KeyPressed = new bool[6];
            for (int i = 0; i < 4; i++) KeyPressed[i] = false;
            

            
            string json = File.ReadAllText("config.json");
            config = JsonSerializer.Deserialize<Config>(json)!;


            InitializeComponent();
            
            this.ClientSize = new Size(config.image_res[0], config.image_res[1]);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            skControl = new SKControl();
            skControl.Size = new Size(config.image_res[0], config.image_res[1]);
            skControl.Dock = DockStyle.Fill;
            skControl.PaintSurface += OnPaintSurface;
            skControl.MouseMove += OnMouseMove;
            skControl.KeyDown += OnKeyDown;
            skControl.KeyUp += OnKeyUp;

            Controls.Add(skControl);
            //






            Vec3[] velo = [new Vec3(0, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 0)];
            Vec3[] acc = [new Vec3(0.0, 0, 0), new Vec3(0.0, 0, 0), new Vec3(0.0, 0, 0)];
            Vec3[] coords =
            [
                new Vec3(-1, -1, -10),
                new Vec3(-1,  1, -10),
                new Vec3(1, -1, -10)
                


            ];
            Vec3[] coords2 =
            [
                new Vec3(-1, -1, -4),
                new Vec3(-1,  1, -4),
                new Vec3(1, -1, -4)


            ];


            Vec3[] colors = new Vec3[3];
            double[] opacity = { 255, 255, 255};
            colors[0] = new Vec3(168, 51, 155);
            colors[1] = new Vec3(79, 64, 64);
            colors[2] = new Vec3(100, 255, 0);
            
            

            t1 = new Triangle( coords, velo, acc, colors, opacity);
            t2 = new Triangle( coords2, velo, acc, colors, opacity);

            image = new Image([t1, t2]);

            //
            int pixelsPerMeter = 10;
            Timer timer = new Timer();
            
            timer.Interval = config.interval;
            timer.Tick += (s, e) =>
            {

                if (KeyPressed[0]) Globals.Camera.move(new Vec3(0,0,-config.movement_speed));
                if (KeyPressed[1]) Globals.Camera.move(new Vec3(-config.movement_speed, 0,0));
                if (KeyPressed[2]) Globals.Camera.move(new Vec3(0,0,config.movement_speed));
                if (KeyPressed[3]) Globals.Camera.move(new Vec3(config.movement_speed, 0,0));
                if (KeyPressed[4]) Globals.Camera.move(new Vec3(0, -config.movement_speed, 0));
                if (KeyPressed[5]) Globals.Camera.move(new Vec3(0, config.movement_speed, 0));
                
                image.update(t1, pixelsPerMeter);  //
                image.update(t2, pixelsPerMeter); //
                image.mapImage();

                Globals.timeElapsed++;
                
                this.Text = $"{Globals.timeElapsed}, {Globals.timeElapsed / 60}";


                skControl.Invalidate();
            };
            timer.Start();
            //
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {

            canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Black);
            image.drawImage(canvas);
            
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Point center = new Point(this.Width / 2, this.Height / 2);
            Cursor.Position = PointToScreen(center);
            Cursor.Hide();
            this.Focus();
        }
        public void OnMouseMove(object sender, MouseEventArgs e)
        {


            int dx = e.X - (this.Width / 2);
            int dy = e.Y - (this.Height / 2);
            //Console.WriteLine(dx);
            //Console.WriteLine(dy);
            
            Globals.Camera.rotate(0, -((double) dy * config.sensitivity));
            Globals.Camera.rotate(1, -((double) dx * config.sensitivity));
            

            Point center = new Point(this.Width / 2, this.Height / 2);
            Cursor.Position = PointToScreen(center);
        }
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {
                KeyPressed[0] = true;
            }
            if (e.KeyCode == Keys.A)
            {
                KeyPressed[1] = true;
            }
            if (e.KeyCode == Keys.S)
            {
                KeyPressed[2] = true;
            }
            if (e.KeyCode == Keys.D)
            {
                KeyPressed[3] = true;
            }
            if (e.KeyCode == Keys.ControlKey)
            {
                KeyPressed[4] = true;
            }
            if (e.KeyCode == Keys.Space)
            {
                KeyPressed[5] = true;
            }

        }
        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {
                KeyPressed[0] = false;
            }
            if (e.KeyCode == Keys.A)
            {
                KeyPressed[1] = false;
            }
            if (e.KeyCode == Keys.S)
            {
                KeyPressed[2] = false;
            }
            if (e.KeyCode == Keys.D)
            {
                KeyPressed[3] = false;
            }
            if (e.KeyCode == Keys.ControlKey)
            {
                KeyPressed[4] = false;
            }
            if (e.KeyCode == Keys.Space)
            {
                KeyPressed[5] = false;
            }
        }


    }
    
}
