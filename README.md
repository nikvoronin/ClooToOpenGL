# .oO@ ClooToOpenGL @Oo.

Live rendering of Mandelbrot fractal with help of .NET, Cloo and SharpGL. Actually, collaboration of OpenCL and OpenGL.

![cloo2opengl_v11](https://cloud.githubusercontent.com/assets/11328666/20339873/ceb54ac6-abef-11e6-9663-ff9e42c66538.png)

- Left Mouse click to center image.<br/>
- Scrool wheeeel to zoom/unzoom.<br/>
- Cursor up/down - Increments/decrements the number of iterations (Shift+ would get x10 speed up)

At start it's looking for NVIDIA graphics card but you can start your own journey from changing this. Just replace the string "NVIDIA" to the "INTEL" or "AMD":

*MainWindow.xaml.cs*

```c#
private void Cloo_Initialize(OpenGLEventArgs args)
{
	[...]
	foreach (var p in ComputePlatform.Platforms)
		foreach(var d in p.Devices)
			if (d.Vendor.ToUpperInvariant().Contains("NVIDIA")) // "INTEL")) // "AMD"))
			[...]
```

## Kernels Here

Since version 1.1 you can edit kernel which you can find under the `/kernels` folder. The kernel file could be edited in isolation of main application.

## Deeper and Darker

![cloo2opengl](https://cloud.githubusercontent.com/assets/11328666/20175314/2d23d5e2-a753-11e6-9e08-58d7c398e394.png)

![cloo2opengl2](https://cloud.githubusercontent.com/assets/11328666/20175474/f178bf84-a753-11e6-9476-3cdb7b282c96.png)
