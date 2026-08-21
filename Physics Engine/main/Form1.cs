namespace Physics_Engine
{
    using Accessibility;
    using Aspose.ThreeD.Entities;
    using Aspose.ThreeD.Formats;
    using OpenTK.Graphics.OpenGL;
    using Physics_Engine.scene;
    using SkiaSharp;
    using SkiaSharp.Views.Desktop;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Text;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices.Marshalling;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text.Json;
    using System.Windows.Forms;


    public partial class Form1 : Form
    {
        public Engine engine;
        public Scene scene;

        
        private SKControl skControl;
        public  SKCanvas canvas;

        public Form1()
        {
            string json1 = File.ReadAllText("config_engine.json");
            Config_Engine engine_config = JsonSerializer.Deserialize<Config_Engine>(json1)!;
            Matrix4 pos = new Matrix4();
            pos[0, 3] = -2;
            pos[1, 3] = 1.3;
            pos[2, 3] = 2;
            Camera camera = new Camera(engine_config);

            SaveConfig saveConfigLive = new SaveConfig();
            bool PreLoad = true;
            int FrameAmount = 1;
            bool IsWrite = false;
            bool IsRead = true;
            bool IsPlayback = true;
            bool IsRepeat = true;

            SaveConfig saveConfigLoad = new SaveConfig(PreLoad, FrameAmount, IsWrite,IsRead,IsPlayback,IsRepeat);
            
            engine = new Engine(new Rasterizer(), camera, engine_config, saveConfigLive);
            engine.Camera.Rotate(0, -200);
            engine.Camera.Rotate(1, 600);
            
                

            InitializeComponent();

            this.ClientSize = new Size(engine.engine_config.Image_res[0], engine.engine_config.Image_res[1]);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            skControl = new SKControl();
            skControl.Size = new Size(engine.engine_config.Image_res[0], engine.engine_config.Image_res[1]);
            skControl.Dock = DockStyle.Fill;
            skControl.PaintSurface += OnPaintSurface;
            skControl.MouseMove += OnMouseMove;
            skControl.KeyDown += OnKeyDown;
            skControl.KeyUp += OnKeyUp;
            Controls.Add(skControl);




            


            
            



            
           







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
                velocities[i] = new Vec3(0, 0, 0);
                accelerations[i] = new Vec3(0, 0, 0);
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
            Vec3[] normals =
            {
                new Vec3(0,0,1),
                new Vec3(0,0,-1),
                new Vec3(-1,0,0),
                new Vec3(1,0,0),
                new Vec3(0,1,0),
                new Vec3(0,-1,0)
            };
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
                isReflective = true,
                lambertian = true
                

            };

            Mesh cube = new Mesh(vertices, atts, shading, normals, faces, indices, true);

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
            
            Light[] lights = { new DistantLight(new Vec3(255, 0, 0), 10000, new Vec3(0, 0, 1))  ,  new DistantLight(new Vec3(255, 255, 255), 1005, new Vec3(01, -0.1, -0.5)) , new PointLight(new Vec3(-2,0,-1), new Vec3(255,255,255), 1000), new PointLight(new Vec3(2, -0.5, 1), new Vec3(0, 255, 255), 1000)  /*, new SpotLight(new Vec3(10,0,0), new Vec3(255,10,255), 1000000, 0.3,new Vec3(0,-0.6,-1)) */};
            Ball ball1 = new Ball(new Vec3(0, 0, -10), 5, at, new ShadingAttributes { facing_ratio = [false], albedo = [new Vec3(1, 1, 1)], isInterpolatedAlbedo = false, isReflective = false, isRefractive = true, refIndex = 1.05});
            Ball ball2 = new Ball(new Vec3(0, 0, -30), 5, at, new ShadingAttributes { facing_ratio = [false], albedo = [new Vec3(1, 0, 0)], isInterpolatedAlbedo = false, isReflective = false });
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
                refIndex = 1,
                lambertian = true
            };
            Mesh import = FileReader.ReadOBJ("C:/Users/Marek/Downloads/kenney_factory-kit_3.0/Models/OBJ format/crane.obj", import_attributes, import_shading, false);
            SceneObject[] objects = { import }; 
            string json2 = File.ReadAllText("config_render.json");
            Config_Render render_config = JsonSerializer.Deserialize<Config_Render>(json2)!;
            Scene scene = new Scene(objects, lights, render_config);


            Timer timer = new Timer();
            timer.Interval = engine.engine_config.Interval;

            SaveConfig saveConfig = new SaveConfig();
            Save save = new Save("C:\\Users\\Marek\\source\\repos\\Physics Engine\\Physics Engine\\animation.bin",[1371, 771], "crane highres");
            engine.Read(save);

            timer.Tick += (s, e) =>
            {
                engine.TickUpdate(scene, save);
                skControl.Invalidate();
                /*
                if(0 == Globals.TimeElapsed % 5)
                {
                    if (engine.Renderer is RayTracer)
                    {
                        engine.Renderer = new Rasterizer();
                        engine.Renderer.SetCamera(engine.Camera);
                        engine.Renderer.SetEngineConfig(engine_config);
                    }
                    else
                    {
                        engine.Renderer = new RayTracer();
                        engine.Renderer.SetCamera(engine.Camera);
                        engine.Renderer.SetEngineConfig(engine_config);
                    }
                }
                */
                
            };
            timer.Start();
            
            
            
        }
        
        

        
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        
        


    }
    
}
