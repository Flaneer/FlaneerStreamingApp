using System;
using FlaneerMediaLib.FlaneerService;
using RestSharp;

namespace MediaLibTests;

public class TestGetRequestObject : RestRequestObject
{
    public override Type? ResponseType => typeof(TestGetResponseObject);

    private static Method method => Method.Get;
    
    public TestGetRequestObject() : base(method)
    {
    }

}
