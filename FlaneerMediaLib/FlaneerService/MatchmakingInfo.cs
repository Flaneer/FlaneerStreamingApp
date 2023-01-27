using Newtonsoft.Json;
using RestSharp;

namespace FlaneerMediaLib.FlaneerService;

/// <summary>
/// Used to send the client information to the server.
/// </summary>
public class MatchmakingInfo : RestRequestObject
{
    /// <inheritdoc />
    public override Type? ResponseType => typeof(MatchmakingResponseObject);
    
    /// <summary>
    /// The client's connection IP address and port.
    /// </summary>
    [JsonProperty]
    private string clientAddress;
    /// <summary>
    /// The server's connection IP address and port.
    /// </summary>
    [JsonProperty]
    private string serverAddress;
    
    /// <summary>
    /// ctor
    /// </summary>
    public MatchmakingInfo(Method method, string clientAddress, string serverAddress) : base(method)
    {
        this.clientAddress = clientAddress;
        this.serverAddress = serverAddress;
    }
}
