using FlaneerMediaLib.FlaneerService;
using RestSharp;
using Xunit;

namespace MediaLibTests;

public class RestSystemTests
{
    [Fact]
    public void TestRestServiceCreation()
    {
        var restService = new RestService();
        Assert.NotNull(restService);
    }

    [Fact]
    public void TestGetResponseObjectPairing()
    {
        var restService = new RestService();

        var requestObject = new TestGetRequestObject();

        restService.GetResponseObject(requestObject, new RestResponse(), out var responseObject);
        
        Assert.IsType<TestGetResponseObject>(responseObject);
    }
}
