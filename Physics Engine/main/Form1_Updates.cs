using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Formats;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Engine
{
    public partial class Form1
    {
        public void TickUpdate()
        {
            
            if ((frameIndex >= config.FrameAmount) && config.PreLoad) RenderFinished = true;
            if (!RenderFinished)
            {
                if (!config.PreLoad)
                {
                    if (keysPressed["W"]) camera.Move(new Vec3(0, 0, -config.Movement_speed));
                    if (keysPressed["A"]) camera.Move(new Vec3(-config.Movement_speed, 0, 0));
                    if (keysPressed["S"]) camera.Move(new Vec3(0, 0, config.Movement_speed));
                    if (keysPressed["D"]) camera.Move(new Vec3(config.Movement_speed, 0, 0));
                    if (keysPressed["CTRL"]) camera.Move(new Vec3(0, -config.Movement_speed, 0));
                    if (keysPressed["SPACE"]) camera.Move(new Vec3(0, config.Movement_speed, 0));
                }


                foreach (SceneObject o in objects) image.Update(o); //
                frames.Add(image.MapImage());
                frameIndex++;
            }

            if (RenderFinished && !WriteFinished && config.IsWrite)
            {
                BinaryWriter writer = new BinaryWriter(File.Open(config.WriteRenderPath, FileMode.Create));
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
                if (RenderFinished && config.IsPlayback)
                {
                    SKBitmap map = new SKBitmap(config.Image_res[0], config.Image_res[1], SKColorType.Bgra8888, SKAlphaType.Premul);
                    System.Runtime.InteropServices.Marshal.Copy(frames[renderFrame], 0, map.GetPixels(), frames[renderFrame].Length);
                    image.DrawImage(canvas, map);
                    if (renderFrame < config.FrameAmount - 1) renderFrame++;


                }
            }


        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Point center = new Point(this.Width / 2, this.Height / 2);
            Cursor.Position = PointToScreen(center);
            Cursor.Hide();
            this.Focus();
        }
    }
}
