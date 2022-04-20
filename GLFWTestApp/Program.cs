
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace GLFWTestApp;

internal static class Program
{
    private static void Main(string[] args)
    {
        GLWindow window = new GLWindow(1280, 720);
        GLEnv env = new GLEnv(window);
        window.StartAppLoop();
    }
}
