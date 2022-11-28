namespace FlaneerMediaLib;

internal class SmartBuffer
{
    public bool CheckedOut;

    public int Length;
    
    public byte[] Buffer = new byte[Int16.MaxValue];
}
