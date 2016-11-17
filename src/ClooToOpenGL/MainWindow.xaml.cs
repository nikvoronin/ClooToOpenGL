using Cloo;
using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace ClooToOpenGL
{
    public partial class MainWindow : Window
    {
        // External OpenCL kernels
        const string KERNEL_FILENAME = "kernels/Mandelbrot.c";
        // Embedded OpenCL kernels
        //const string KERNEL_FILENAME = "ClooToOpenGL.kernels.Mandelbrot.c";
        Render render;
        ComputeDevice cDevice;

        public MainWindow()
        {
            InitializeComponent();
        }

        uint width = 1920;
        uint height = 1080;

        Stopwatch timer;
        long delta = 0;
        long lastTicks = 0;
        private void SGL_Draw(object sender, OpenGLEventArgs args)
        {
            delta = timer.ElapsedTicks - lastTicks;
            lastTicks = timer.ElapsedTicks;
            Title = $"Cloo to OpenGL — {cDevice.Platform.Name} {width}x{height} i{render?.maxIter} | {delta / 10000}ms, {10000000 / delta}fps";

            render.ConfigureKernel();
            render.ExecuteKernel();
            render.ReadResult();

            OpenGL gl = args.OpenGL;

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 1);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, 4, (int)width, (int)height, 0, OpenGL.GL_RGBA, OpenGL.GL_UNSIGNED_BYTE, render.h_resultBuf);

            gl.ClearColor(0f, 0f, 0f, 0f);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho(-1f, 1f, 1f, -1f, 1f, 100f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.LoadIdentity();
            gl.Begin(OpenGL.GL_QUADS);
                gl.TexCoord(0f, 1f);
                gl.Vertex(-1f, -1f, -1f);

                gl.TexCoord(0f, 0f);
                gl.Vertex(-1f, 1f, -1f);

                gl.TexCoord(1f, 0f);
                gl.Vertex(1f, 1f, -1f);

                gl.TexCoord(1f, 1f);
                gl.Vertex(1f, -1f, -1f);
            gl.End();

            gl.Flush();
        }

        private void SGL_Initialized(object sender, OpenGLEventArgs args)
        {
            Cloo_Initialize(args);

            OpenGL gl = args.OpenGL;
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Disable(OpenGL.GL_DEPTH_TEST);

            timer = new Stopwatch();
            timer.Start();
        }

        private void Cloo_Initialize(OpenGLEventArgs args)
        {
            string kernelSource = File.ReadAllText(KERNEL_FILENAME); //LoadEmbeddedFile(KERNEL_FILENAME);

            foreach (var p in ComputePlatform.Platforms)
                foreach(var d in p.Devices)
                    if (d.Vendor.ToUpperInvariant().Contains("NVIDIA")) // "INTEL")) // "AMD"))
                    {
                        cDevice = d;
                        break;
                    }

            if (render != null)
                render =
                    new Render(
                        cDevice,
                        kernelSource,
                        width, height,
                        width * height / 10,
                        render.reMin, render.reMax, render.imMin, render.imMax,
                        render.maxIter
                        );
            else
                render =
                    new Render(
                        cDevice,
                        kernelSource,
                        width, height,
                        width * height / 10
                        );

            render.BuildKernels();
            render.AllocateBuffers();
            render.ConfigureKernel();
        }

        /// <summary>
        /// If you are embedd OpenCL kernels into the assembly: Build Action -> Embedded Resource
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string LoadEmbeddedFile(string filename)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filename);
            TextReader reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        private void SGL_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // positive Delta - zoom in
            // negative Delta - zoom out
            float ax = (float)Math.Abs(render.reMax - render.reMin) * (e.Delta > 0 ? -0.1f : 0.1f);
            float ay = (float)Math.Abs(render.imMax - render.imMin) * (e.Delta > 0 ? -0.1f : 0.1f);

            render.reMin -= ax;
            render.reMax += ax;
            render.imMin -= ay;
            render.imMax += ay;

        }

        private void SGL_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(sender as UIElement);
            Size elsz = (sender as UIElement).DesiredSize;
            float dx = (float)(pos.X / elsz.Width - 0.5f) * 2.0f;
            float dy = (float)(pos.Y / elsz.Height - 0.5f) * 2.0f;
            
            float lx2 = (float)Math.Abs(render.reMax - render.reMin) / 2.0f * dx;
            float ly2 = (float)Math.Abs(render.imMax - render.imMin) / 2.0f * dy;
            render.reMin += lx2;
            render.reMax += lx2;
            render.imMin += ly2;
            render.imMax += ly2;
        }

        private void SGL_Resized(object sender, OpenGLEventArgs args)
        {
            Size sz = (sender as UIElement).RenderSize;
            width = (uint)sz.Width;
            height = (uint)sz.Height;

            Cloo_Initialize(args);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Up:      
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        render.maxIter += 10;
                    else
                        render.maxIter++;
                    break;
                case Key.Down:
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        if (render.maxIter > 10)
                            render.maxIter -= 10;
                    else
                        if (render.maxIter > 1)
                            render.maxIter--;
                    break;
            }
        }
    }
}
