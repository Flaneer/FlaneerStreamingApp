using Silk.NET.OpenGL;

namespace GLDisplayApp;

public class Shader
{
    private readonly uint handle;
    private readonly GL gl;

    public Shader(GLEnv gl, string vertexPath, string fragmentPath)
    {
        this.gl = gl.Gl;

        uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
        uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);
        handle = this.gl.CreateProgram();
        this.gl.AttachShader(handle, vertex);
        this.gl.AttachShader(handle, fragment);
        this.gl.LinkProgram(handle);
        this.gl.GetProgram(handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {this.gl.GetProgramInfoLog(handle)}");
        }
        this.gl.DetachShader(handle, vertex);
        this.gl.DetachShader(handle, fragment);
        this.gl.DeleteShader(vertex);
        this.gl.DeleteShader(fragment);
    }

    public void Use()
    {
        gl.UseProgram(handle);
    }

    public void SetUniform(string name, int value)
    {
        int location = gl.GetUniformLocation(handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        gl.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        int location = gl.GetUniformLocation(handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        gl.Uniform1(location, value);
    }

    public void Dispose()
    {
        gl.DeleteProgram(handle);
    }

    private uint LoadShader(ShaderType type, string path)
    {
        string src = File.ReadAllText(path);
        uint handle = gl.CreateShader(type);
        gl.ShaderSource(handle, src);
        gl.CompileShader(handle);
        string infoLog = gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }
}