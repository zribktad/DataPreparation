using Steeltoe.Common.Discovery;
using Steeltoe.Discovery;

public class FakeDiscoveryClient : IDiscoveryClient
{
    public IServiceInstance FakeServiceInstance { get; set; }

    public IList<IServiceInstance> GetInstances(string serviceId)
    {
        return [FakeServiceInstance];
    }

    public string Description { get; }
    public IList<string> Services { get; }
    public IServiceInstance GetLocalServiceInstance()
    {
        return null;
    }

    public Task ShutdownAsync()
    {
        throw new NotImplementedException();
    }
}