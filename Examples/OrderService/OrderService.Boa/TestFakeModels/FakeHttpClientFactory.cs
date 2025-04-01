namespace OrderService.Boa.TestFakeModels;

public class FakeHttpClientFactory : IHttpClientFactory
{
    public HttpClient FakeHttpClient { get; set; }
    
    public HttpClient CreateClient(string name)
    {
        return FakeHttpClient;
    }
}