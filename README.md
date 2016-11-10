# .0O@ ClooToOpenGL @Oo.

Live rendering of Mandelbrot fractal with help of .NET, Cloo and SharpGL. Actually, collaboration of OpenCL and OpenGL.

- Left Mouse click to center image.<br/>
- Scrool wheeeel to zoom/unzoom.

At start it's looking for NVIDIA graphics card but you can start your own journey from changing this. Just replace the string "NVIDIA" to the "INTEL" or "AMD":

*MainWindow.xaml.cs*

```c#
private void Cloo_Initialize(OpenGLEventArgs args)
{
[...]
	foreach (var p in ComputePlatform.Platforms)
		if (p.Vendor.ToUpperInvariant().Contains("INTEL"))
[...]
```

