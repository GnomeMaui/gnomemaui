using System.Runtime.InteropServices;

namespace Microsoft.Maui.Graphics.Platform;
/// <summary>
/// OpenGL and EGL P/Invoke bindings for Linux.
/// </summary>
public static partial class GL
{
	/// <summary>
	/// Return the value or values of a selected parameter.
	/// Reference: <see href="https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGet.xhtml">OpenGL ES GLSL Specification</see>
	/// </summary>
	/// <param name="pname">Specifies the parameter value to be returned (e.g., GL_FRAMEBUFFER_BINDING).</param>
	/// <param name="data">Returns the value or values of the specified parameter.</param>
	[LibraryImport("libGL.so.1", EntryPoint = "glGetIntegerv")]
	internal static partial void GetIntegerv(int pname, [Out, MarshalAs(UnmanagedType.LPArray)] int[] data);
}

/// <summary>
/// EGL (Embedded-System Graphics Library) P/Invoke bindings for Linux.
/// </summary>
public static partial class EGL
{
	/// <summary>
	/// Return a GL or an EGL extension function pointer.
	/// Reference: https://www.khronos.org/registry/EGL/sdk/docs/man/html/eglGetProcAddress.xhtml
	/// </summary>
	/// <param name="procname">Specifies the name of the function to return (null-terminated string).</param>
	/// <returns>
	/// Returns the address of the extension function named by procname. 
	/// A NULL return value indicates the function does not exist.
	/// </returns>
	[LibraryImport("libEGL.so.1", EntryPoint = "eglGetProcAddress")]
	public static partial IntPtr GetProcAddress([MarshalAs(UnmanagedType.LPStr)] string procname);
}
