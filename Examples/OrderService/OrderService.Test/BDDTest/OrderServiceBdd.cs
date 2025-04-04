using Boa.Constrictor.Screenplay;
using DataPreparation.Analyzers.Test;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OrderService.BoaTest.Factories.SQLite;
using OrderService.BoaTest.OrderService.Abilities;
using OrderService.BoaTest.OrderService.Questions;
using OrderService.BoaTest.OrderService.Tasks;
using OrderService.BoaTest.TestFakeModels;
using OrderService.DTO;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;
using OrderService.Test.Domain.Factories.SQLite;
using Shouldly;
using Steeltoe.Discovery;

namespace DataPreparation.Testing;

[DataPreparationFixture]
public class OrderServiceBdd : IDataPreparationTestServices, IDataPreparationLogger
{

    public void DataPreparationServices(IServiceCollection serviceCollection)
    {

        SqliteConnection databaseConnection = new("DataSource=:memory:");
        databaseConnection.Open();
        serviceCollection.AddDbContext<OrderServiceContext>(options =>
            options.UseSqlite(databaseConnection,
                sqliteOptions => sqliteOptions
                    .MigrationsHistoryTable("__EFMigrationsHistory")
                    .MigrationsAssembly("OrderServiceBdd")));

        serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        serviceCollection.AddScoped<IOrderService, OrderService.Services.OrderService>();
        serviceCollection.AddScoped<IOrderManagementService, OrderManagementService>();
        serviceCollection.AddScoped<IOrderStatusService,  OrderStatusService>();
        serviceCollection.AddScoped<IOrderItemService, OrderItemService>();
        serviceCollection.AddScoped<ICustomerService,  CustomerService>();
        serviceCollection.AddScoped<IDiscoveryClient, FakeDiscoveryClient>();
        serviceCollection.AddScoped<IHttpClientFactory, FakeHttpClientFactory>();

        var context = serviceCollection.BuildServiceProvider().GetRequiredService<OrderServiceContext>();
        context.Database.EnsureCreated();
    }
    

    public ILoggerFactory InitializeDataPreparationTestLogger()
    {
        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddDebug();
            builder.AddConsole();
        });

    }
    
    [DataPreparationTest]
    public async Task CreateOrder_FullOrderDTO_ReturnsOrder()
    {
     
    }
    

}