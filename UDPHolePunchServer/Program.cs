using FlaneerMediaLib;
using FlaneerMediaLib.UnreliableDataChannel.HolePunching;

namespace UDPHolePunchServer;

internal class Program
{
    static void Main(string[] args)
    {
        CommandLineArgumentStore.CreateAndRegister(args);
        HolePunchServer server = new HolePunchServer();
    }
}
