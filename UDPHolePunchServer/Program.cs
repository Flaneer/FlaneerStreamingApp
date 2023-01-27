using FlaneerMediaLib;
using FlaneerMediaLib.FlaneerService;
using FlaneerMediaLib.UnreliableDataChannel.HolePunching;

namespace UDPHolePunchServer;

internal class Program
{
    static void Main(string[] args)
    {
        CommandLineArgumentStore.CreateAndRegister(args);
        
        var restService = new RestService();
        ServiceRegistry.AddService(restService);
        
        HolePunchServer server = new HolePunchServer();
    }
}
