namespace Physics_Engine
{
    using Accessibility;
    using Aspose.ThreeD.Entities;
    using OpenTK.Graphics.OpenGL;
    using SkiaSharp;
    using SkiaSharp.Views.Desktop;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Text;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices.Marshalling;
    using System.Security.Policy;
    using System.Text.Json;
    using System.Windows.Forms;


    public partial class Form1 : Form
    {
        Config config { get; set; }
        
        private SKControl skControl;
        private bool[] KeyPressed;
        readonly public SceneObject[] objects;
        public  SKCanvas canvas;
        Image image;
        bool RenderFinished = false;
        bool WriteFinished = false;
        int frameIndex = 0;
        List<byte[]> frames = new List<byte[]>();
        int renderFrame = 0;

        public Form1()
        {

            //Globals.Camera.Rotate(0,250);
            Globals.Camera.Rotate(1,-750);

            //
            KeyPressed = new bool[6];
            for (int i = 0; i < 4; i++) KeyPressed[i] = false;



            string json = File.ReadAllText("config.json");
            config = JsonSerializer.Deserialize<Config>(json)!;


            string? dir = Path.GetDirectoryName(config.ReadRenderPath);

            if(!string.IsNullOrEmpty(dir) && config.IsRead)
            {
                RenderFinished = true;
                BinaryReader reader = new BinaryReader(File.Open(config.ReadRenderPath, FileMode.Open));

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    int length = reader.ReadInt32();  // read frame size
                    byte[] frame = reader.ReadBytes(length); // read frame data

                    frames.Add(frame);
                }
                reader.Close();
            }
            



            InitializeComponent();

            this.ClientSize = new Size(config.Image_res[0], config.Image_res[1]);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            skControl = new SKControl();
            skControl.Size = new Size(config.Image_res[0], config.Image_res[1]);
            skControl.Dock = DockStyle.Fill;
            skControl.PaintSurface += OnPaintSurface;
            skControl.MouseMove += OnMouseMove;
            skControl.KeyDown += OnKeyDown;
            skControl.KeyUp += OnKeyUp;

            Controls.Add(skControl);
            //







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
            bool[] ratio = {false, false, false, false, false, false};

            int[] indices = {
            0,  1,  2,  3,   // front
            4,  5,  6,  7,   // back
            8,  9,  10, 11,  // left
            12, 13, 14, 15,  // right
            16, 17, 18, 19,  // top
            20, 21, 22, 23   // bottom
        };

            bool[] onesided = { false, false, false, false, false, false };

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
                albedo = albedo,
                textureT = new double[24],
                textureV = new double[24]

            };
            ShadingAttributes shading = new ShadingAttributes
            {
                facing_ratio = ratio,
                albedo = albedoShading,
                isInterpolatedAlbedo = false,
                isReflective = true

            };

            Mesh cube = new Mesh(vertices, atts, shading, faces, indices, onesided);

            Vec3[] c = { new Vec3(255, 255, 255), new Vec3(255, 255, 255), new Vec3(255, 255, 255) };
            Vec3[] v = { new Vec3(-0.1, 0, 0), new Vec3(-0.1, 0, 0), new Vec3(-0.1, 0, 0) };
            Vec3[] a = { new Vec3(-0.1, 0, 0), new Vec3(-0.1, 0, 0), new Vec3(-0.1, 0, 0) };
            double[] o = new double[24];
            VertexAttributes at = new VertexAttributes
            {
                colors = c,
                velocity = v,
                acceleration = a,
                opacity = o,
                
            };
            Vec3 CheckerBoard(Vec3 point)
            {
                int x = (int)Math.Floor(point.X * 0.1);
                int z = (int)Math.Floor(point.Z * 0.1);
                return (x + z) % 2 == 0 ? new Vec3(1,1,1) : new Vec3(0,0,0);
            }
            
            Light[] lights = {/* new DistantLight(new Vec3(255, 0, 0), 3, new Vec3(0, 0, 1))  ,  new DistantLight(new Vec3(255, 255, 255), 05, new Vec3(01, -0.1, -0.5)) ,*/ new PointLight(new Vec3(0,7,20.1), new Vec3(255,255,255), 10000) /*, new SpotLight(new Vec3(10,0,0), new Vec3(255,10,255), 1000000, 0.3,new Vec3(0,-0.6,-1))*/};
            Ball ball1 = new Ball(new Vec3(0, 0, -10), at, new ShadingAttributes { facing_ratio = [false], albedo = [new Vec3(1, 1, 1)], isInterpolatedAlbedo = false, isReflective = false, isRefractive = true, refIndex = 1.05}, 5);
            Ball ball2 = new Ball(new Vec3(0, 0, -30), at, new ShadingAttributes { facing_ratio = [false], albedo = [new Vec3(1, 0, 0)], isInterpolatedAlbedo = false, isReflective = false }, 5);
            Plane plane1 = new Plane(at, new ShadingAttributes { facing_ratio = [false], albedo = [new Vec3(1, 1, 1)], isInterpolatedAlbedo = false, isReflective = false, textureFunc = CheckerBoard }, new Vec3(0,1,0), new Vec3(0,-20,0));
            Plane plane2 = new Plane(at, new ShadingAttributes { facing_ratio = [false], albedo = [new Vec3(1, 1, 1)], isInterpolatedAlbedo = false, isReflective = true, oneSided = false }, new Vec3(1,0,0), new Vec3(-40,0,0));
            Disk disk = new Disk(new Vec3(-15,5,0), at, new ShadingAttributes { facing_ratio = [false], albedo = [new Vec3(1, 1, 1)], isInterpolatedAlbedo = false }, new Vec3(0,1,1), 10);
            VertexAttributes import_attributes = new VertexAttributes
            {
                colors = [new Vec3(255,255,255)],
                albedo = [new Vec3(1,1,1)],
                velocity = [new Vec3(0,0,0)],
                acceleration = [new Vec3(0, 0, 0)],
                textureT = [0],
                textureV = [0],
                opacity = [255]
            };
            ShadingAttributes import_shading = new ShadingAttributes
            {
                isInterpolatedAlbedo = false,
                albedo = [new Vec3(1,1,1)],
                facing_ratio = [false],
                oneSided = false,
                isReflective = false,
                isRefractive = false,
                textureFunc = null,
                refIndex = 1
            };
            Mesh import = FileReader.ReadOBJ("C:/Users/Marek/Downloads/kenney_factory-kit_3.0/Models/OBJ format/crane.obj", import_attributes, import_shading);
            SceneObject[] objects = { import };


            image = new Image(objects, lights);




            



            

            Timer timer = new Timer();
            timer.Interval = config.Interval;


            timer.Tick += (s, e) =>
            {
                if ((frameIndex >= config.FrameAmount) && config.PreLoad) RenderFinished = true;
                if (!RenderFinished)
                {
                    if(!config.PreLoad)
                    {
                        if (KeyPressed[0]) Globals.Camera.Move(new Vec3(0, 0, -config.Movement_speed));
                        if (KeyPressed[1]) Globals.Camera.Move(new Vec3(-config.Movement_speed, 0, 0));
                        if (KeyPressed[2]) Globals.Camera.Move(new Vec3(0, 0, config.Movement_speed));
                        if (KeyPressed[3]) Globals.Camera.Move(new Vec3(config.Movement_speed, 0, 0));
                        if (KeyPressed[4]) Globals.Camera.Move(new Vec3(0, -config.Movement_speed, 0));
                        if (KeyPressed[5]) Globals.Camera.Move(new Vec3(0, config.Movement_speed, 0));
                    }
                    

                    foreach (SceneObject o in objects) image.Update(o); //
                    frames.Add(image.MapImage());
                    frameIndex++;
                }

                if(RenderFinished && !WriteFinished && config.IsWrite)
                {
                    BinaryWriter writer =new BinaryWriter(File.Open(config.WriteRenderPath, FileMode.Create));
                    string? dirw = Path.GetDirectoryName(config.WriteRenderPath);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    foreach (byte[] frame in frames)
                    {
                        writer.Write(frame.Length);
                        writer.Write(frame);
                    }
                    WriteFinished = true;
                    writer.Close();
                }

                Globals.TimeElapsed++;
                this.Text = $"{Globals.TimeElapsed}";

                

                skControl.Invalidate();
                
                
                
                    

                

            };
            timer.Start();
            
            
            
        }
        
        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            
            canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Black);
            if (config.PreLoad == false)
            {
                SKBitmap map = new SKBitmap(config.Image_res[0], config.Image_res[1], SKColorType.Bgra8888, SKAlphaType.Premul);
                System.Runtime.InteropServices.Marshal.Copy(frames[frameIndex - 1], 0, map.GetPixels(), frames[frameIndex - 1].Length);
                if (frames.Count != 0) image.DrawImage(canvas, map);
            } 
            else
            {
                if(RenderFinished && config.IsPlayback)
                {
                    SKBitmap map = new SKBitmap(config.Image_res[0], config.Image_res[1], SKColorType.Bgra8888, SKAlphaType.Premul);
                    System.Runtime.InteropServices.Marshal.Copy(frames[renderFrame], 0, map.GetPixels(), frames[renderFrame].Length);
                    image.DrawImage(canvas, map);
                    if(renderFrame < config.FrameAmount - 1) renderFrame++;
                    

                }
            }
            
            
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
            if (config.PreLoad) return;

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
            if (config.PreLoad) return;
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
            if (config.PreLoad) return;
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
