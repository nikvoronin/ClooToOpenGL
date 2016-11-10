using Cloo;
using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace ClooToOpenGL
{
    public partial class MainWindow : Window
    {
        const string KERNEL_FILENAME = "ClooToOpenGL.kernels.Mandelbrot.c";
        Render render;
        ComputePlatform cPlatform;

        public MainWindow()
        {
            InitializeComponent();
        }

        uint width = 1280;
        uint height = 1024;

        Stopwatch timer;
        long delta = 0;
        long lastTicks = 0;
        private void SharpGL_Draw(object sender, OpenGLEventArgs args)
        {
            delta = timer.ElapsedTicks - lastTicks;
            lastTicks = timer.ElapsedTicks;
            Title = $"Cloo to OpenGL — {delta / 10000}ms, {10000000 / delta}fps";

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

        private void SharpGL_Initialized(object sender, OpenGLEventArgs args)
        {
            Cloo_Initialize(args);

            OpenGL gl = args.OpenGL;
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            timer = new Stopwatch();
            timer.Start();
        }

        private void Cloo_Initialize(OpenGLEventArgs args)
        {
            string kernelSource = LoadEmbeddedFile(KERNEL_FILENAME);

            foreach (var p in ComputePlatform.Platforms)
                if (p.Vendor.ToUpperInvariant().Contains("INTEL"))
                {
                    cPlatform = p;
                    break;
                }

            render = new Render(cPlatform, kernelSource, width, height, width * height / 10);

            render.BuildKernels();
            render.AllocateBuffers();
            render.ConfigureKernel();
        }

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

        private void OpenGLControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
    }
}
