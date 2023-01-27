using RestSharp;

namespace FlaneerMediaLib.FlaneerService;

/// <summary>
/// Response object for the GetSessionId method
/// </summary>
public class MatchmakingResponseObject : RestResponseObject
{
    /// <summary>
    /// ctor
    /// </summary>
    public MatchmakingResponseObject(RestResponse parameters) : base(parameters)
    {
        
    }
}