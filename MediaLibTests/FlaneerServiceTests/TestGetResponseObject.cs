using FlaneerMediaLib.FlaneerService;
using RestSharp;

namespace MediaLibTests;

public class TestGetResponseObject : RestResponseObject
{
    public TestGetResponseObject(RestResponse parameters) : base(parameters)
    {
    }
}
