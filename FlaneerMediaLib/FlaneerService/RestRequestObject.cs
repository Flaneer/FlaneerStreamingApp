using Newtonsoft.Json;
using RestSharp;

namespace FlaneerMediaLib.FlaneerService;

/// <summary>
/// Base class for all rest request objects
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public abstract class RestRequestObject
{
    private Method method;
    private string resource => ToJson();
    
    /// <summary>
    /// The response type (if any) that the request expects 
    /// </summary>
    public abstract Type? ResponseType { get; }

    /// <summary>
    /// ctor
    /// </summary>
    protected RestRequestObject(Method method)
    {
        this.method = method;
    }

    /// <summary>
    /// ctor
    /// </summary>
    public RestRequest ToRestRequest()
    {
        var request = new RestRequest(resource, method);
        request.RequestFormat = DataFormat.Json;
        request.AddBody(this);
        return request;
    }

    private string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }
}
