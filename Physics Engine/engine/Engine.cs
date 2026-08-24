using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Formats;
using Physics_Engine.scene;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Engine
{ 
    public class Engine
    {
        List<byte[]> frames = new List<byte[]>();
        private int frameIndex = 0;
        private bool RenderFinished = false;
        private bool WriteFinished = false;
        private int renderFrame;
        private bool isNullSave = false;
        
        public SaveConfig saveConfig;
        public IRenderEngine Renderer;
        public Camera Camera;
        public Config_Engine engine_config;
        
        Dictionary<string, bool> keysPressed = new Dictionary<string, bool>();
        public Engine(IRenderEngine Renderer, Camera Camera, Config_Engine engine_config, SaveConfig saveConfig)
        {
            this.saveConfig = saveConfig;
            this.Renderer = Renderer;
            this.Camera = Camera;
            this.engine_config = engine_config;
            this.Renderer.SetCamera(Camera);
            this.Renderer.SetEngineConfig(engine_config);
            keysPressed.Add("W", false);
            keysPressed.Add("A", false);
            keysPressed.Add("S", false);
            keysPressed.Add("D", false);
            keysPressed.Add("CTRL", false);
            keysPressed.Add("SPACE", false);
            
        }
        public void Paint(SKCanvas canvas, Save? save)
        {
            


            if (!saveConfig.PreLoad)
            {
                SKBitmap map = new SKBitmap(engine_config.Image_res[0], engine_config.Image_res[1], SKColorType.Bgra8888, SKAlphaType.Premul);
                if (frames.Count != 0) System.Runtime.InteropServices.Marshal.Copy(frames[frameIndex - 1], 0, map.GetPixels(), frames[frameIndex - 1].Length);
                if (frames.Count != 0) canvas.DrawBitmap(map,0,0);
                return;
            }
            
            if (RenderFinished && saveConfig.IsPlayback)
            {
                if (isNullSave) return;
                if (save is null) { isNullSave = true; Console.WriteLine("Save is null"); return; }
                SKBitmap map = new SKBitmap(save.resolution[0], save.resolution[1], SKColorType.Bgra8888, SKAlphaType.Premul);
                System.Runtime.InteropServices.Marshal.Copy(frames[renderFrame], 0, map.GetPixels(), frames[renderFrame].Length);
                canvas.DrawBitmap(map, 0, 0);
                if (renderFrame < saveConfig.FrameAmount - 1) renderFrame++;


            }
            
        }
        public Point? MouseMove(MouseEventArgs e)
        {
            if (saveConfig.PreLoad) return null;

            //Console.Write("rotation");
            int dx = e.X - (engine_config.Image_res[0] / 2);
            int dy = e.Y - (engine_config.Image_res[1] / 2);

            Camera.Rotate(0, dy);
            Camera.Rotate(1, dx);

            Point center = new Point(engine_config.Image_res[0] / 2, engine_config.Image_res[1] / 2);
            return center;
            
        }
        public bool KeyDown(KeyEventArgs e)
        {
            if (saveConfig.PreLoad) return true;
            switch (e.KeyCode)
            {
                case Keys.W:
                    keysPressed["W"] = true;
                    return false;
                case Keys.A:
                    keysPressed["A"] = true;
                    return false; ;
                case Keys.S:
                    keysPressed["S"] = true;
                    return false;
                case Keys.D:
                    keysPressed["D"] = true;
                    return false;
                case Keys.ControlKey:
                    keysPressed["CTRL"] = true;
                    return false;
                case Keys.Space:
                    keysPressed["SPACE"] = true;
                    return false;
                    
            }
            return false;
        }
        public bool KeyUp(KeyEventArgs e)
        {
            if (saveConfig.PreLoad) return true;
            switch (e.KeyCode)
            {
                case Keys.W:
                    keysPressed["W"] = false;
                    return false;
                case Keys.A:
                    keysPressed["A"] = false;
                    return false;
                case Keys.S:
                    keysPressed["S"] = false;
                    return false;
                case Keys.D:
                    keysPressed["D"] = false;
                    return false;
                case Keys.ControlKey:
                    keysPressed["CTRL"] = false;
                    return false;
                case Keys.Space:
                    keysPressed["SPACE"] = false;
                    return false;

            }
            return false;


        }
        /*
        public void Read()
        {
            string? dir = Path.GetDirectoryName(engine_config.ReadRenderPath);

            if (!string.IsNullOrEmpty(dir) && engine_config.IsRead)
            {
                RenderFinished = true;
                BinaryReader reader = new BinaryReader(File.Open(engine_config.ReadRenderPath, FileMode.Open));

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    int length = reader.ReadInt32();  // read frame size
                    byte[] frame = reader.ReadBytes(length); // read frame data

                    frames.Add(frame);
                }
                reader.Close();
            }
        }
        */
        public void Read(Save? save)
        {
            if (save is null) return;
            string? dir = Path.GetDirectoryName(save.filepath);

            if (!string.IsNullOrEmpty(dir) && saveConfig.IsRead)
            {
                RenderFinished = true;
                BinaryReader reader = new BinaryReader(File.Open(save.filepath, FileMode.Open));

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    int length = reader.ReadInt32();  // read frame size
                    byte[] frame = reader.ReadBytes(length); // read frame data

                    frames.Add(frame);
                }
                reader.Close();
            }
        }
        public void TickUpdate(Scene scene, Save? save)
        {

            if ((frameIndex >= saveConfig.FrameAmount) && saveConfig.PreLoad) RenderFinished = true;
            if (!RenderFinished)
            {
                if (!saveConfig.PreLoad)
                {
                    if (keysPressed["W"]) Camera.Move(new Vec3(0, 0, -scene.render_config.Movement_speed));
                    if (keysPressed["A"]) Camera.Move(new Vec3(-scene.render_config.Movement_speed, 0, 0));
                    if (keysPressed["S"]) Camera.Move(new Vec3(0, 0, scene.render_config.Movement_speed));
                    if (keysPressed["D"]) Camera.Move(new Vec3(scene.render_config.Movement_speed, 0, 0));
                    if (keysPressed["CTRL"]) Camera.Move(new Vec3(0, -scene.render_config.Movement_speed, 0));
                    if (keysPressed["SPACE"]) Camera.Move(new Vec3(0, scene.render_config.Movement_speed, 0));
                }


                foreach (SceneObject o in scene.objects) UpdateScene(o); //
                frames.Add(Renderer.RenderImage(scene));
                frameIndex++;
            }

            if (RenderFinished && !WriteFinished && saveConfig.IsWrite)
            {
                BinaryWriter writer = new BinaryWriter(File.Open(save.filepath, FileMode.Create));
                string? dir = Path.GetDirectoryName(save.filepath);
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
            //this.Text = $"{Globals.TimeElapsed}";
        }
        public void UpdateScene(SceneObject o, double pixelsPerMeter = 50)
        {

            double t = (float)engine_config.Interval / 1000;
            foreach (Vec3 vertex in o.transformedVertices)
            {
                
                
               
                //ObjectToWorldMatrix[0, 3] += t * o.attributes.velocity[j].X * pixelsPerMeter;
                //ObjectToWorldMatrix[1, 3] += t * o.attributes.velocity[j].Y * pixelsPerMeter;
                //ObjectToWorldMatrix[2, 3] += t * o.attributes.velocity[j].Z * pixelsPerMeter;
                //Vec4 trans = ObjectToWorldMatrix * new Vec4(o.vertices[j].X, o.vertices[j].Y, o.vertices[j].Z, 1);
                //o.vertices[j].X = trans.X; o.vertices[j].Y = trans.Y; o.vertices[j].Z = trans.Z;


            }
            /*
            if (o is Mesh mesh)
            {
                
                for (int j = 0; j < mesh.nTriangles; j++)
                {
                    mesh.triangles[j].vertices[0] = mesh.vertices[(int)mesh.tIndices[j, 0]];
                    mesh.triangles[j].vertices[1] = mesh.vertices[(int)mesh.tIndices[j, 1]];
                    mesh.triangles[j].vertices[2] = mesh.vertices[(int)mesh.tIndices[j, 2]];
                }
            }
            */


        }
    }
}
