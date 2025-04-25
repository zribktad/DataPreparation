using System.Net;
using Bogus;
using DataPreparation.Data;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using OrderService.BoaTest.OrderService.Tasks;
using OrderService.BoaTest.TestFakeModels;
using OrderService.DTO;
using Steeltoe.Common.Discovery;
using Steeltoe.Discovery;
using Steeltoe.Discovery.Eureka;

namespace OrderService.BoaTest.PreparedData;

[PreparationClassFor(typeof(UpdateOrderStatusTask))]
public class UpdateOrderStatusTaskData(IDiscoveryClient discoveryClient, IHttpClientFactory httpClientFactory)
    : IBeforePreparation
{
    private readonly FakeDiscoveryClient? _discoveryClient = discoveryClient as FakeDiscoveryClient;

    private readonly FakeHttpClientFactory? _httpClientFactory = httpClientFactory as FakeHttpClientFactory;

    [UpData]
    public void UpData()
    {
        //DiscoveryClient
        var serviceInstance = SetupDiscovery();
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

    private static IServiceInstance SetupDiscovery()
    {
        var mockServiceInstance = new Mock<IServiceInstance>();
        mockServiceInstance.Setup(instance => instance.Host).Returns("Test");
        mockServiceInstance.Setup(instance => instance.Port).Returns(0);
        var serviceInstance = mockServiceInstance.Object;
        return serviceInstance;
    }
    [DownData]
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

    public static AddressDTO Generate()
    {
        return _faker.Generate();
    }

    public static List<AddressDTO> Generate(int count)
    {
        return _faker.Generate(count);
    }
}