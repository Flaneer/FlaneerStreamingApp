using System.Diagnostics;
using System.Net.Sockets;
using FlaneerMediaLib;
using FlaneerMediaLib.VideoDataTypes;

namespace UDPToWinFormTest;

public partial class MainForm : Form
{
   private Bitmap? screenCapture;

   private object lockObject = new object();
   int it = 0;
   
    public MainForm()
    {
        InitializeComponent();
    }
    
    private void FormDemo_Shown(object sender, EventArgs e)
    {
        ServiceRegistry.TryGetService(out IVideoSource videoSource);
        
        DisplayLoop(videoSource);
    }

    private void DisplayLoop(IVideoSource videoSource)
    {
        while (Visible)
        {
            Application.DoEvents();
            
            try
            {
                var frameIn = videoSource.GetFrame();
                ManagedVideoFrame? frame = frameIn as ManagedVideoFrame;
                if(frame == null || frame.Stream.Length == 0)
                    continue;

                var pathName = $"out-{it}.h264";
                File.WriteAllBytes(pathName, frame.Stream.ToArray());
                
                using (Process myProcess = new Process())
                {
                    myProcess.StartInfo.FileName = "ffmpeg.exe";
                    myProcess.StartInfo.Arguments = $" -i {pathName} test-{++it}.jpeg";
                    myProcess.StartInfo.UseShellExecute = false;
                    myProcess.StartInfo.RedirectStandardOutput = true;

                    myProcess.Start();

                    myProcess.OutputDataReceived += (sender, args) => Console.WriteLine($"{args.Data}");

                    myProcess.WaitForExit();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            
            if (File.Exists($"test-{it}.jpeg"))
            {
                screenCapture = new Bitmap($"test-{it}.jpeg");
                var smallerBitmap = ResizeBitmap(screenCapture, DisplayImage.Width, DisplayImage.Height);
                DisplayImage.Image = Image.FromHbitmap(smallerBitmap.GetHbitmap());
            }
        }
    }
    
    public Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
    {
        Bitmap result = new Bitmap(width, height);
        using (Graphics g = Graphics.FromImage(result))
        {
            g.DrawImage(bmp, 0, 0, width, height);
        }
 
        return result;
    }
}