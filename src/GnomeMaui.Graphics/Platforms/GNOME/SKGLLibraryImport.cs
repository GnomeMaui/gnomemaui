using System.Runtime.InteropServices;

namespace Microsoft.Maui.Graphics.Platform;
/// <summary>
/// OpenGL and EGL P/Invoke bindings for Linux.
/// </summary>
public static partial class GL
{
	internal const int GL_TEXTURE_2D = 0x0DE1;
	internal const int GL_ARRAY_BUFFER = 0x8892;
	internal const int GL_ELEMENT_ARRAY_BUFFER = 0x8893;
	internal const int GL_FRAMEBUFFER_BINDING = 0x8CA6;
	internal const int GL_STENCIL_BITS = 0x0D57;
	internal const int GL_SAMPLES = 0x80A9;

	/// <summary>
	/// Return the value or values of a selected parameter.
	/// Reference: <see href="https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGet.xhtml">OpenGL ES GLSL Specification</see>
	/// </summary>
	/// <param name="pname">Specifies the parameter value to be returned (e.g., GL_FRAMEBUFFER_BINDING).</param>
	/// <param name="data">Returns the value or values of the specified parameter.</param>
	[LibraryImport("libGL.so.1", EntryPoint = "glGetIntegerv")]
	internal static partial void GetIntegerv(int pname, [Out, MarshalAs(UnmanagedType.LPArray)] int[] data);

	/// <summary>
	/// Block until all GL execution is complete.
	/// Reference: <see href="https://registry.khronos.org/OpenGL-Refpages/gl4/html/glFinish.xhtml">OpenGL ES GLSL Specification</see>
	/// </summary>
	[LibraryImport("libGL.so.1", EntryPoint = "glFinish")]
	internal static partial void Finish();

	/// <summary>
	/// Set the viewport.
	/// Reference: <see href="https://registry.khronos.org/OpenGL-Refpages/gl4/html/glViewport.xhtml">OpenGL ES GLSL Specification</see>
	/// </summary>
	[LibraryImport("libGL.so.1", EntryPoint = "glViewport")]
	internal static partial void Viewport(int x, int y, int width, int height);

	/// <summary>
	/// Define the scissor box.
	/// Reference: <see href="https://registry.khronos.org/OpenGL-Refpages/gl4/html/glScissor.xhtml">OpenGL ES GLSL Specification</see>
	/// </summary>
	[LibraryImport("libGL.so.1", EntryPoint = "glScissor")]
	internal static partial void Scissor(int x, int y, int width, int height);

	/// <summary>
	/// Bind a framebuffer to a framebuffer target.
	/// Reference: <see href="https://registry.khronos.org/OpenGL-Refpages/gl4/html/glBindFramebuffer.xhtml">OpenGL ES GLSL Specification</see>
	/// </summary>
	[LibraryImport("libGL.so.1", EntryPoint = "glBindFramebuffer")]
	internal static partial void BindFramebuffer(int target, uint framebuffer);

	/// <summary>
	/// Bind a texture to a texture target.
	/// Reference: <see href="https://registry.khronos.org/OpenGL-Refpages/gl4/html/glBindTexture.xhtml">OpenGL ES GLSL Specification</see>
	/// </summary>
	[LibraryImport("libGL.so.1", EntryPoint = "glBindTexture")]
	internal static partial void BindTexture(int target, uint texture);

	/// <summary>
	/// Bind a buffer object to the specified buffer binding point.
	/// Reference: <see href="https://registry.khronos.org/OpenGL-Refpages/gl4/html/glBindBuffer.xhtml">OpenGL ES GLSL Specification</see>
	/// </summary>
	[LibraryImport("libGL.so.1", EntryPoint = "glBindBuffer")]
	internal static partial void BindBuffer(int target, uint buffer);
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
