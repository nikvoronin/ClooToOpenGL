using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;

namespace ClooToOpenTK
{
    public class MainWindow : GameWindow
    {
        Render cloo;

        public MainWindow()
        {
            Title = Const.APP_NAME;
            VSync = VSyncMode.Off;
            Width = Const.DISPLAY_XGA_W;
            Height = Const.DISPLAY_XGA_H;

            KeyUp += MainWindow_KeyUp;
            KeyDown += MainWindow_KeyDown;
            MouseWheel += MainWindow_MouseWheel;
            MouseUp += MainWindow_MouseUp;
        }

        private void MainWindow_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    cloo.maxIter += e.Shift ? 10u : 1u;
                    break;
                case Key.Down:
                    if (e.Shift)
                    {
                        if (cloo.maxIter > 10)
                            cloo.maxIter -= 10;
                    }
                    else
                    {
                        if (cloo.maxIter > 1)
                            cloo.maxIter--;
                    }
                    break;
            }
        }

        private void MainWindow_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Exit();
                    break;
                case Key.F11:
                    ToggleFullscreen();
                    break;
            }
        }

        private void ToggleFullscreen()
        {
            DisplayDevice defaultDisplayDevice = DisplayDevice.GetDisplay(DisplayIndex.Default);

            if (WindowState == WindowState.Fullscreen)
            {
                WindowState = WindowState.Normal;
                CursorVisible = true;
                defaultDisplayDevice.RestoreResolution();
            }
            else
            {
                WindowState = WindowState.Fullscreen;
                CursorVisible = false;
                defaultDisplayDevice
                    .ChangeResolution(
                        Const.DISPLAY_FULLHD_W, Const.DISPLAY_FULLHD_H,
                        Const.DISPLAY_BITPERPIXEL,
                        Const.DISPLAY_REFRESH_RATE);
            }
        }

        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            float dx = ((float)e.X / Width - 0.5f) * 2.0f;
            float dy = ((float)e.Y / Height - 0.5f) * 2.0f;

            float lx2 = Math.Abs(cloo.reMax - cloo.reMin) / 2.0f * dx;
            float ly2 = Math.Abs(cloo.imMax - cloo.imMin) / 2.0f * dy;
            cloo.reMin += lx2;
            cloo.reMax += lx2;
            cloo.imMin += ly2;
            cloo.imMax += ly2;
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // positive Delta - zoom in
            // negative Delta - zoom out
            float ax = Math.Abs(cloo.reMax - cloo.reMin) * (e.Delta > 0 ? -0.1f : 0.1f);
            float ay = Math.Abs(cloo.imMax - cloo.imMin) * (e.Delta > 0 ? -0.1f : 0.1f);

            cloo.reMin -= ax;
            cloo.reMax += ax;
            cloo.imMin -= ay;
            cloo.imMax += ay;
        }

        internal void UpdateCloo()
        {
            cloo = Render.CreateCloo((uint)Width, (uint)Height, cloo);
        }

        protected override void OnLoad(EventArgs e)
        {
            UpdateCloo();
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
        }

        protected override void OnResize(EventArgs e)
        {
            UpdateCloo();
            GL.Viewport(0, 0, Width, Height);
        }

        double s1_timer = 0;    // smooth fps printing

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            float delta = (float)e.Time;

            s1_timer += e.Time;
            if (s1_timer > 1f)
            {
                Title = $"{Const.APP_NAME}, {Const.RELEASE_DATE} — {cloo?.ComputePlatformName}: {(delta * 1000).ToString("0.")}ms, {(1.0 / delta).ToString("0")}fps // i{cloo?.maxIter}";
                s1_timer = 0;
            }

            cloo.ConfigureKernel();
            cloo.ExecuteKernel();
            cloo.ReadResult();

            GL.BindTexture(TextureTarget.Texture2D, 1);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Four, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, cloo.h_resultBuf);

            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1f, 1f, 1f, -1f, 1f, 100f);
            GL.MatrixMode(MatrixMode.Modelview);

            GL.LoadIdentity();
            GL.Begin(PrimitiveType.Quads);
                GL.TexCoord2(0f, 1f);
                GL.Vertex3(-1f, -1f, -1f);

                GL.TexCoord2(0f, 0f);
                GL.Vertex3(-1f, 1f, -1f);

                GL.TexCoord2(1f, 0f);
                GL.Vertex3(1f, 1f, -1f);

                GL.TexCoord2(1f, 1f);
                GL.Vertex3(1f, -1f, -1f);
            GL.End();

            GL.Flush();

            SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnClosed(e);
        }
    }
}
