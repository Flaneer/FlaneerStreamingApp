using Silk.NET.OpenGL;

namespace GLFWTestApp;

public class Texture
{
    private uint handle;
    private GL gl;

    public unsafe Texture(GLEnv GLIn, string path)
    {
        gl = GLIn.Gl;

        handle = gl.GenTexture();
        Bind();

        //Loading an image using imagesharp.
        SetTextureFromImage(path);

        SetParameters();
    }

    internal unsafe void SetTextureFromImage(string path)
    {
    }

    public unsafe Texture(GL gl, Span<byte> data, uint width, uint height)
    {
        //Saving the gl instance.
        this.gl = gl;

        //Generating the opengl handle;
        handle = this.gl.GenTexture();
        Bind();

        //We want the ability to create a texture using data generated from code aswell.
        fixed (void* d = &data[0])
        {
            //Setting the data of a texture.
            this.gl.TexImage2D(TextureTarget.Texture2D, 0, (int) InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
            SetParameters();
        }
    }

    private void SetParameters()
    {
        //Setting some texture perameters so the texture behaves as expected.
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Linear);
        //Generating mipmaps.
        gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        //When we bind a texture we can choose which textureslot we can bind it to.
        gl.ActiveTexture(textureSlot);
        gl.BindTexture(TextureTarget.Texture2D, handle);
    }

    public void Dispose()
    {
        //In order to dispose we need to delete the opengl handle for the texure.
        gl.DeleteTexture(handle);
    }
}