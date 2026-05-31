namespace Physics_Engine
{
    using Accessibility;
    using Microsoft.VisualBasic.ApplicationServices;
    using OpenTK.Graphics.OpenGL;
    using SkiaSharp;
    using SkiaSharp.Views.Desktop;
    using System.Collections.Generic;
    using System.Drawing.Text;
    using System.Runtime.InteropServices.Marshalling;
    using System.Security.Policy;
    using System.Text.Json;
    using System.Windows.Forms;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;


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
            
            this.ClientSize = new Size(config.Image_Res[0], config.Image_Res[1]);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            skControl = new SKControl();
            skControl.Size = new Size(config.Image_Res[0], config.Image_Res[1]);
            skControl.Dock = DockStyle.Fill;
            skControl.PaintSurface += OnPaintSurface;
            skControl.MouseMove += OnMouseMove;
            skControl.KeyDown += OnKeyDown;
            skControl.KeyUp += OnKeyUp;

            Controls.Add(skControl);
            //






            Vec3[] velo = [new Vec3(0, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 0)];
            Vec3[] acc = [new Vec3(0.0, 0, 0), new Vec3(0.0, 0, 0), new Vec3(0.0, 0, 0)];
            
            Vec3[] co1 =
            [
                new Vec3(-1, -1, -10),
                new Vec3(-1,  1, -10),
                new Vec3(1, -1, -10)
                


            ];
            Vec3[] cos2 =
            [
                new Vec3(-1, -1, -4),
                new Vec3(-1,  1, -4),
                new Vec3(1, -1, -4)


            ];


            Vec3[] cs = new Vec3[3];
            double[] op = { 255, 255, 255};
            cs[0] = new Vec3(168, 51, 155);
            cs[1] = new Vec3(255, 255, 255);
            cs[2] = new Vec3(100, 255, 0);

            VertexAttributes ats = new VertexAttributes()
            {
                colors = cs,
                opacity = op,
                velocity = velo,
                acceleration = acc
            };


            t1 = new Triangle( co1, new Vec3(0, 0, -1), ats,  new ShadingAttributes());
            t2 = new Triangle( cos2, new Vec3(0, 0, -1), ats, new ShadingAttributes());





            Vec3[] vertices = {
            // front face
            new Vec3(-1, -1, -3),
            new Vec3(-1,  1, -3),
            new Vec3( 1,  1, -3),
            new Vec3( 1, -1, -3),
            // back face
            new Vec3( 1, -1, -5),
            new Vec3( 1,  1, -5),
            new Vec3(-1,  1, -5),
            new Vec3(-1, -1, -5),
            // left face
            new Vec3(-1, -1, -5),
            new Vec3(-1,  1, -5),
            new Vec3(-1,  1, -3),
            new Vec3(-1, -1, -3),
            // right face
            new Vec3( 1, -1, -3),
            new Vec3( 1,  1, -3),
            new Vec3( 1,  1, -5),
            new Vec3( 1, -1, -5),
            // top face
            new Vec3(-1,  1, -3),
            new Vec3(-1,  1, -5),
            new Vec3( 1,  1, -5),
            new Vec3( 1,  1, -3),
            // bottom face
            new Vec3(-1, -1, -5),
            new Vec3(-1, -1, -3),
            new Vec3( 1, -1, -3),
            new Vec3( 1, -1, -5),


        };

            int[] faces = { 4, 4, 4, 4, 4, 4 }; // 6 faces, 4 vertices each
            

            int[] indices = {
            0,  1,  2,  3,   // front
            4,  5,  6,  7,   // back
            8,  9,  10, 11,  // left
            12, 13, 14, 15,  // right
            16, 17, 18, 19,  // top
            20, 21, 22, 23   // bottom
        };

            

            // simple colors per vertex - each face a different color
            Vec3[] colors = {
            new Vec3(255, 255, 0),   new Vec3(255, 0, 0),   new Vec3(255, 255, 0),   new Vec3(255, 0, 0),   // front red
            new Vec3(0, 255, 0),   new Vec3(0, 255, 0),   new Vec3(0, 255, 0),   new Vec3(0, 255, 0),   // back green
            new Vec3(0, 0, 255),   new Vec3(0, 0, 255),   new Vec3(0, 0, 255),   new Vec3(0, 0, 255),   // left blue
            new Vec3(255, 255, 0), new Vec3(255, 255, 0), new Vec3(255, 255, 0), new Vec3(255, 255, 0), // right yellow
            new Vec3(255, 0, 255), new Vec3(255, 0, 255), new Vec3(255, 0, 255), new Vec3(255, 0, 255), // top magenta
            new Vec3(0, 255, 255), new Vec3(0, 255, 255), new Vec3(0, 255, 255), new Vec3(0, 255, 255), // bottom cyan
        };

            // fill remaining attributes with zeros
            Vec3[] velocities = new Vec3[24];
            Vec3[] accelerations = new Vec3[24];



            double[] opacities = new double[24];
            for (int i = 0; i < 24; i++)
            {
                velocities[i] = new Vec3(0, 0.01, 0);
                accelerations[i] = new Vec3(0, 0.1, 0);
                opacities[i] = 255;
            }
            Vec3[] normals =
            {
                new Vec3(0,0,1),
                new Vec3(0,0,-1),
                new Vec3(-1,0,0),
                new Vec3(1,0,0),
                new Vec3(0,1,0),
                new Vec3(0,-1,0)
            };

            Vec3[] albedoShading =
            [
                new Vec3(1, 0, 0),
                new Vec3(0, 1, 0),
                new Vec3(0, 0, 1),
                new Vec3(1, 1, 0),
                new Vec3(1, 0, 1),
                new Vec3(0, 1, 1)
            ];

            Vec3[] albedo = new Vec3[24];
            for (int i = 0; i < 24; i++)
            {
                albedo[i] = colors[i] / 255;
            }
            VertexAttributes atts = new VertexAttributes
            {
                colors = colors,
                velocity = velocities,
                acceleration = accelerations,
                opacity = opacities,
                

            };
            ShadingAttributes shading = new ShadingAttributes
            {
                onesided = false
            };
            

            Mesh cube = new Mesh(vertices, atts, shading, normals, faces, indices);
            Mesh import = FileReader.ReadOBJ("C:/Users/Marek/Downloads/kenney_factory-kit_3.0/Models/OBJ format/crane.obj");
            Console.WriteLine(import.nTriangles);
            Object[] objects = [import];
            
            image = new Image(objects);

            //
            int pixelsPerMeter = 10;
            Timer timer = new Timer();
            
            timer.Interval = config.Interval;
            timer.Tick += (s, e) =>
            {

                if (KeyPressed[0]) Globals.Camera.Move(new Vec3(0,0,-config.Movement_Speed));
                if (KeyPressed[1]) Globals.Camera.Move(new Vec3(-config.Movement_Speed, 0,0));
                if (KeyPressed[2]) Globals.Camera.Move(new Vec3(0,0,config.Movement_Speed));
                if (KeyPressed[3]) Globals.Camera.Move(new Vec3(config.Movement_Speed, 0,0));
                if (KeyPressed[4]) Globals.Camera.Move(new Vec3(0, -config.Movement_Speed, 0));
                if (KeyPressed[5]) Globals.Camera.Move(new Vec3(0, config.Movement_Speed, 0));
                
                image.Update(t1, pixelsPerMeter);  //
                image.Update(t2, pixelsPerMeter); //
                image.MapImage();

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
            image.DrawImage(canvas);
            
            
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
            
            Globals.Camera.Rotate(0,dy);
            Globals.Camera.Rotate(1,dx);
            

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
