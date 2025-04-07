using DataPreparation.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.BoaTest.TestFakeModels;
using OrderService.Models;
using OrderService.Repository;
using OrderService.Services;
using Steeltoe.Discovery;

namespace OrderService.Test.Domain;

public class SqLiteDataPreparationFixture : IDataPreparationTestServices, IDataPreparationLogger
{
    public void DataPreparationServices(IServiceCollection serviceCollection)
    {
        SqliteConnection databaseConnection = new("DataSource=:memory:");
        databaseConnection.Open();
        serviceCollection.AddDbContext<OrderServiceContext>(options =>
            options.UseSqlite(databaseConnection,
                sqliteOptions => sqliteOptions
                    .MigrationsHistoryTable("__EFMigrationsHistory")
                    .MigrationsAssembly("OrderServiceBddTest")));

        serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        serviceCollection.AddScoped<IOrderService, OrderService.Services.OrderService>();
        serviceCollection.AddScoped<IOrderManagementService, OrderManagementService>();
        serviceCollection.AddScoped<IOrderStatusService, OrderStatusService>();
        serviceCollection.AddScoped<IOrderItemService, OrderItemService>();
        serviceCollection.AddScoped<ICustomerService, CustomerService>();
        serviceCollection.AddScoped<IDiscoveryClient, FakeDiscoveryClient>();
        serviceCollection.AddScoped<IHttpClientFactory, FakeHttpClientFactory>();

        var context = serviceCollection.BuildServiceProvider().GetRequiredService<OrderServiceContext>();
        context.Database.EnsureCreated();
    }


    public ILoggerFactory InitializeDataPreparationTestLogger()
    {
        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddDebug();
            builder.AddConsole();
        });
    }
}