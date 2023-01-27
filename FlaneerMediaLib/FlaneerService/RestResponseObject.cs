using RestSharp;

namespace FlaneerMediaLib.FlaneerService;

/// <summary>
/// utility class for guaranteeing that a certain constructor is provided in a class
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class MustInitialize<T>
{
    /// <summary>
    /// ctor
    /// </summary>
    public MustInitialize(T parameters)
    {

    }
}

/// <summary>
/// Base class for all rest response objects
/// </summary>
public abstract class RestResponseObject : MustInitialize<RestResponse>
{
    /// <summary>
    /// ctor
    /// </summary>
    public RestResponseObject(RestResponse parameters) : base(parameters)
    {
        
    }
}
