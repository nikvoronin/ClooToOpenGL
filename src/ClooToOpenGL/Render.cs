using System;
using System.Runtime.InteropServices;
using Cloo;

namespace ClooToOpenGL
{
    public class Render : IDisposable
    {
        public ComputePlatform clPlatform;
        public ComputeContext clContext;
        public ComputeContextPropertyList clProperties;
        public ComputeKernel clKernel;
        public ComputeProgram clProgram;
        public ComputeCommandQueue clCommands;

        public ComputeBuffer<Vector4ui> cbuf_Rng;
        public ComputeBuffer<byte> cbuf_Result;
        public byte[] h_resultBuf;
        private GCHandle gc_resultBuffer;

        uint workers;
        private uint width;
        private uint height;
        public float reMin = -2.0f;
        public float reMax = 1.0f;
        public float imMin = -1.0f;
        public float imMax = 1.0f;
        public uint maxIter = 200;

        public string KernelName = "Mandelbrot";

        public Render(
            ComputeDevice cDevice,
            string kernelSource,
            uint width, uint height,
            uint workers,
            float reMin = -2.0f,
            float reMax = 1.0f,
            float imMin = -1.0f,
            float imMax = 1.0f,
            uint maxIter = 200)
        {
            this.width = width;
            this.height = height;
            this.workers = workers;
            this.reMin = reMin;
            this.reMax = reMax;
            this.imMin = imMin;
            this.imMax = imMax;
            this.maxIter = maxIter;

            clPlatform = cDevice.Platform;
            clProperties = new ComputeContextPropertyList(clPlatform);
            clContext = new ComputeContext(clPlatform.Devices, clProperties, null, IntPtr.Zero);
            clCommands = new ComputeCommandQueue(clContext, cDevice, ComputeCommandQueueFlags.None);
            clProgram = new ComputeProgram(clContext, new string[] { kernelSource });

            h_resultBuf = new byte[width * height * 4];
            gc_resultBuffer = GCHandle.Alloc(h_resultBuf, GCHandleType.Pinned);

            int i = kernelSource.IndexOf("__kernel");
            if (i > -1)
            {
                int j = kernelSource.IndexOf("(", i);
                if (j > -1)
                {
                    string raw = kernelSource.Substring(i + 8, j - i - 8);
                    string[] parts = raw.Trim().Split(' ');
                    for (int k = parts.Length - 1; k != 0; k--)
                    {
                        if (!string.IsNullOrEmpty(parts[k]))
                        {
                            KernelName = parts[k];
                            break;
                        } // if
                    } // for k
                } // if j
            } // if i
        }

        public void BuildKernels()
        {
            string msg = null;
            try
            {
                clProgram.Build(null, "", null, IntPtr.Zero);
                clKernel = clProgram.CreateKernel(KernelName);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            if (clKernel == null)
                throw new Exception(msg);
        }

        public void AllocateBuffers()
        {
            Random rnd = new Random((int)DateTime.UtcNow.Ticks);

            Vector4ui[] seeds = new Vector4ui[workers];
            for (int i = 0; i < workers; i++)
                seeds[i] =
                    new Vector4ui
                    {
                        x = (ushort)rnd.Next(),
                        y = (ushort)rnd.Next(),
                        z = (ushort)rnd.Next(),
                        w = (ushort)rnd.Next()
                    };

            cbuf_Rng =
                new ComputeBuffer<Vector4ui>(
                    clContext,
                    ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer,
                    seeds);

            cbuf_Result =
                new ComputeBuffer<byte>(
                    clContext,
                    ComputeMemoryFlags.ReadOnly,
                    width * height * 4);
        }

        public void ConfigureKernel()
        {
            clKernel.SetValueArgument(0, width);
            clKernel.SetValueArgument(1, height);
            clKernel.SetValueArgument(2, reMin);
            clKernel.SetValueArgument(3, reMax);
            clKernel.SetValueArgument(4, imMin);
            clKernel.SetValueArgument(5, imMax);
            clKernel.SetValueArgument(6, maxIter);
            clKernel.SetMemoryArgument(7, cbuf_Rng);
            clKernel.SetMemoryArgument(8, cbuf_Result);
        }

        public void ExecuteKernel()
        {
            clCommands.Execute(clKernel, null, new long[] { workers }, null, null);
        }

        public void FinishKernel()
        {
            clCommands.Finish();
        }

        public void ReadResult()
        {
            clCommands.Read(cbuf_Result, true, 0, width * height * 4, gc_resultBuffer.AddrOfPinnedObject(), null);
            clCommands.Finish();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                clCommands.Dispose();
                clKernel.Dispose();
                clProgram.Dispose();
                clContext.Dispose();
                cbuf_Result.Dispose();
                cbuf_Rng.Dispose();
            }
        }
    }
}
