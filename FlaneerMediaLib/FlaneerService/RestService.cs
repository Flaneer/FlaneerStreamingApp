using RestSharp;

namespace FlaneerMediaLib.FlaneerService;

/// <summary>
/// Service for communicating with the Flaneer streaming API
/// </summary>
public class RestService : IService
{
    private RestClient restClient;

    private const string baseUrl = "https://string-id.execute-api.eu-west-1.amazonaws.com/streaming/";
    
    /// <summary>
    /// ctor
    /// </summary>
    public RestService()
    {
        restClient = new RestClient(baseUrl);
    }
    
    /// <summary>
    /// Performs a get request to the Flaneer streaming API
    /// </summary>
    public async Task<RestResponseObject?> Get<T>(T requestObject) where T : RestRequestObject
    {
        var response = await restClient.GetAsync(requestObject.ToRestRequest());
        return GetResponseObject(requestObject, response, out var responseObject) ? responseObject : null;
    }

    /// <summary>
    /// Returns a response object that matches the request object expected type
    /// </summary>
    public bool GetResponseObject(RestRequestObject requestObject, RestResponse responseIn, out RestResponseObject? responseOut) 
    {
        if(requestObject.ResponseType == null || !requestObject.ResponseType.IsAssignableTo(typeof(RestResponseObject)))
            throw new ArgumentException($"ResponseType ({requestObject.ResponseType}) must be a subclass of RestResponseObject");

        responseOut = Activator.CreateInstance(requestObject.ResponseType, responseIn) as RestResponseObject;
        if (responseOut != null)
        {
            return true;
        }
        return false;
    }
}
