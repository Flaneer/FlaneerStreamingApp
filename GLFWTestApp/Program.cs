namespace GLFWTestApp;

internal static class Program
{
    private static void Main(string[] args)
    {
        GLWindow window = new GLWindow(1920, 1080);
        GLEnv env = new GLEnv(window);
        window.StartAppLoop();
    }
}
