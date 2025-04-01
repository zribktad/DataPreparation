using System.Net;
using Bogus;
using DataPreparation.Data;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using OrderService.Boa.OrderService.Tasks;
using OrderService.Boa.TestFakeModels;
using OrderService.DTO;
using Steeltoe.Common.Discovery;
using Steeltoe.Discovery;
using Steeltoe.Discovery.Eureka;

namespace OrderService.Boa.PreparedData;


[PreparationClassFor(typeof(UpdateOrderStatusTask))]
public class UpdateOrderStatusTaskData : IBeforePreparation
{

    private FakeDiscoveryClient? _discoveryClient;

    private FakeHttpClientFactory? _httpClientFactory;
   
    public UpdateOrderStatusTaskData(IDiscoveryClient discoveryClient,IHttpClientFactory httpClientFactory)
    {
        
        _discoveryClient = discoveryClient as FakeDiscoveryClient;
        _httpClientFactory = httpClientFactory as FakeHttpClientFactory;
    }


    public void UpData()
    {
        //DiscoveryClient
        var mockServiceInstance = new Mock<IServiceInstance>();
        mockServiceInstance.Setup(instance => instance.Host).Returns("Test");
        mockServiceInstance.Setup(instance => instance.Port).Returns(0);
        var serviceInstance = mockServiceInstance.Object;
        _discoveryClient.FakeServiceInstance = serviceInstance;
        
        //HttpClientFactory
        
      
        var fakeAddress = AddressFaker.Generate();
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                var jsonResponse = JsonConvert.SerializeObject(fakeAddress);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                };
            });
        
        var mockClient = new HttpClient(mockHandler.Object);

        _httpClientFactory.FakeHttpClient = mockClient;

    }
    
    public void DownData()
    { 
    }
    
    
}


public static class AddressFaker
{
    private static readonly Faker<AddressDTO> _faker = new Faker<AddressDTO>()
        .RuleFor(a => a.Street, f => f.Address.StreetAddress())
        .RuleFor(a => a.City, f => f.Address.City())
        .RuleFor(a => a.ZipCode, f => f.Address.ZipCode())
        .RuleFor(a => a.State, f => f.Address.State());

    public static AddressDTO Generate() => _faker.Generate();
    
    public static List<AddressDTO> Generate(int count) => _faker.Generate(count);
}