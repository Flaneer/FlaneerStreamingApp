using System;
using FlaneerMediaLib.FlaneerService;
using Newtonsoft.Json;
using RestSharp;

namespace MediaLibTests;

public class TestGetRequestObject : RestRequestObject
{
    public override Type? ResponseType => typeof(TestGetResponseObject);

    private static Method method => Method.Get;
    
    [JsonProperty]
    public string testString;
    
    [JsonProperty]
    public int testInt;
    
    [JsonProperty]
    public ushort testUshort;

    public TestGetRequestObject() : base(method)
    {
    }

}
