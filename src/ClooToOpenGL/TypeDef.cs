using System.Runtime.InteropServices;

namespace ClooToOpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4ui
    {
        public uint x;
        public uint y;
        public uint z;
        public uint w;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4f
    {
        public float x;
        public float y;
        public float z;
        public float w;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2f
    {
        public float x;
        public float y;
    };
}
