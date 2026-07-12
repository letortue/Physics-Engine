namespace Physics_Engine
{
    using Accessibility;
    using Aspose.ThreeD.Entities;
    using OpenTK.Graphics.OpenGL;
    using Physics_Engine.Engine;
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

        private IRenderEngine engine;
        private Camera camera;

        
        Dictionary<string, bool> keysPressed;
        private SKControl skControl;
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
            camera = new Camera(new Matrix4());
            engine = new RayTracer(camera);
            Dictionary<string, bool> keysPressed = new Dictionary<string, bool>();

            /*
            string json = File.ReadAllText("config.json");
            config = JsonSerializer.Deserialize<Config>(json)!;
            */

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
            Mesh import = FileReader.ReadOBJ("C:/Users/Marek/Downloads/kenney_factory-kit_3.0/Models/OBJ format/crane.obj", import_attributes, import_shading, true);
            SceneObject[] objects = { import };



            Timer timer = new Timer();
            timer.Interval = config.Interval;


            timer.Tick += (s, e) =>
            {
                TickUpdate();
                skControl.Invalidate();

            };
            timer.Start();
            
            
            
        }
        
        

        /*
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        */
        


    }
    
}
