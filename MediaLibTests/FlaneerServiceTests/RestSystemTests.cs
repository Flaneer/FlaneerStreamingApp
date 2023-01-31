using FlaneerMediaLib.FlaneerService;
using Newtonsoft.Json;
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

    [Fact]
    public void TestRequestObjectToJson()
    {
        var requestObject = new TestGetRequestObject();

        var requestObjectTestString = "test";
        requestObject.testString = requestObjectTestString;

        var requestObjectTestInt = 1;
        requestObject.testInt = requestObjectTestInt;

        ushort requestObjectTestUshort = 2;
        requestObject.testUshort = requestObjectTestUshort;
        
        var json = requestObject.ToJson();

        var deserializedObject = JsonConvert.DeserializeObject<TestGetRequestObject>(json);
        
        Assert.Equal(requestObjectTestString, deserializedObject.testString);
        Assert.Equal(requestObjectTestInt, deserializedObject.testInt);
        Assert.Equal(requestObjectTestUshort, deserializedObject.testUshort);
    }
    
    [Fact]
    public void TestRequestObjectToRestRequest()
    {
        var requestObject = new TestGetRequestObject();

        var requestObjectTestString = "test";
        requestObject.testString = requestObjectTestString;

        var requestObjectTestInt = 1;
        requestObject.testInt = requestObjectTestInt;

        ushort requestObjectTestUshort = 2;
        requestObject.testUshort = requestObjectTestUshort;
        
        var restRequest = requestObject.ToRestRequest();
        Assert.NotNull(restRequest);
        Assert.Equal(Method.Get, restRequest.Method);

        var json = restRequest.Resource;
        var deserializedObject = JsonConvert.DeserializeObject<TestGetRequestObject>(json);
        
        Assert.Equal(requestObjectTestString, deserializedObject.testString);
        Assert.Equal(requestObjectTestInt, deserializedObject.testInt);
        Assert.Equal(requestObjectTestUshort, deserializedObject.testUshort);
    }
}
