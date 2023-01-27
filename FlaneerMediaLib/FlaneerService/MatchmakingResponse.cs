using Newtonsoft.Json;
using RestSharp;

namespace FlaneerMediaLib.FlaneerService;

/// <summary>
/// Response object for the GetSessionId method
/// </summary>
public class MatchmakingResponse : RestResponseObject
{
    /// <summary>
    /// Session ID
    /// </summary>
    public ushort SessionId { get; init; }
    
    /// <summary>
    /// ctor
    /// </summary>
    public MatchmakingResponse(RestResponse parameters) : base(parameters)
    {
        var deserializedObject = JsonConvert.DeserializeObject<MatchmakingResponse>(parameters.Content);
        SessionId = deserializedObject.SessionId;
    }
}
