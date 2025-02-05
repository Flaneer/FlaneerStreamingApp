﻿using FlaneerMediaLib.VideoDataTypes;

namespace FlaneerMediaLib.VideoStreaming;

/// <summary>
/// Uses local frames to emulate a UDP video source
/// </summary>
public class LocalFramesVideoSource : IVideoSource
{
    private string framesPath = "";
    private string frameNameTemplate = "";
    private int numberOfLocalFrames;
    private int currentFrame;
    
    private string FileNameFromIdx(int idx) => Path.Join(framesPath, frameNameTemplate.Replace("{}", $"{idx}"));

    /// <inheritdoc />
    public ICodecSettings CodecSettings { get; private set; }
    
    /// <inheritdoc />
    public FrameSettings FrameSettings { get; private set; }
    
    /// <inheritdoc />
    public bool Init(FrameSettings frameSettingsIn, ICodecSettings codecSettingsIn)
    {
        FrameSettings = frameSettingsIn;
        CodecSettings = codecSettingsIn;

        if (!LoadInfoFromCLI())
            return false;

        //TODO: load in files

        return true;
    }

    private bool LoadInfoFromCLI()
    {
        ServiceRegistry.TryGetService<CommandLineArgumentStore>(out var clArgStore);
        var clParams = clArgStore.GetParams(CommandLineArgs.UseLocalFrames);
        try
        {
            framesPath = clParams[0];
            frameNameTemplate = clParams[1];
            numberOfLocalFrames = int.Parse(clParams[2]);
        }
        catch (Exception e)
        {
            //TODO: Log the error here!
            return false;
        }

        return true;
    }

    private void IterateCurrentFrame()
    {
        currentFrame++;
        if (currentFrame == numberOfLocalFrames)
            currentFrame = 0;
    }
    
    /// <inheritdoc />
    public bool GetFrame(out IVideoFrame frame)
    {
        IterateCurrentFrame();
        frame = new ManagedVideoFrame
        {
            Codec = VideoCodec.H264,
            Height = (short) FrameSettings.Height,
            Width = (short) FrameSettings.Width,
            Stream = new MemoryStream(File.ReadAllBytes(FileNameFromIdx(currentFrame)))
        };
        return true;
    }
    
    /// <inheritdoc />
    public void Dispose() { }
}
