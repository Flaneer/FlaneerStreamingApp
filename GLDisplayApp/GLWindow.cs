using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace GLDisplayApp;

public class GLWindow
{
    internal readonly IWindow window;
    
    public GLWindow(int width, int height)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = "Flaneer";
        window = Window.Create(options);

        window.Load += OnLoad;
    }

    public void StartAppLoop()
    {
        window.Run();
    }

    private void OnLoad()
    {
        IInputContext input = window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }
    }

    private void KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        if (arg2 == Key.Escape)
        {
            window.Close();
        }
    }
}