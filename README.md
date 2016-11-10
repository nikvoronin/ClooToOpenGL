# .oO@ ClooToOpenGL @Oo.

Live rendering of Mandelbrot fractal with help of .NET, Cloo and SharpGL. Actually, collaboration of OpenCL and OpenGL.

![cloo2opengl](https://cloud.githubusercontent.com/assets/11328666/20175314/2d23d5e2-a753-11e6-9e08-58d7c398e394.png)

- Left Mouse click to center image.<br/>
- Scrool wheeeel to zoom/unzoom.

At start it's looking for NVIDIA graphics card but you can start your own journey from changing this. Just replace the string "NVIDIA" to the "INTEL" or "AMD":

*MainWindow.xaml.cs*

```c#
private void Cloo_Initialize(OpenGLEventArgs args)
{
[...]
	foreach (var p in ComputePlatform.Platforms)
		if (p.Vendor.ToUpperInvariant().Contains("NVIDIA"))
[...]
```

![cloo2opengl2](https://cloud.githubusercontent.com/assets/11328666/20175474/f178bf84-a753-11e6-9476-3cdb7b282c96.png)
