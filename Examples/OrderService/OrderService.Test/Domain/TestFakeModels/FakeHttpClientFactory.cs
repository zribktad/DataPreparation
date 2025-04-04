namespace OrderService.BoaTest.TestFakeModels;

public class FakeHttpClientFactory : IHttpClientFactory
{
    public HttpClient FakeHttpClient { get; set; }
    
    public HttpClient CreateClient(string name)
    {
        return FakeHttpClient;
    }
}