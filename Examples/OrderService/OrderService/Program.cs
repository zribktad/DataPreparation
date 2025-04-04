using OrderService.Models;
using System;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using OrderService.Controllers;
using OrderService.Repository;
using OrderService.Services;
using Steeltoe.Discovery.Client;
using Steeltoe.Management.Endpoint;
using Microsoft.AspNetCore.HttpLogging;
using Steeltoe.Extensions.Configuration;
using Graceterm;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();
builder.Services.AddScoped<IOrderManagementService, OrderManagementService>();
builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.AddAllActuators();

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddEnvironmentVariables();

// Determine which database to use based on configuration
var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? builder.Configuration["DatabaseProvider"] ?? "postgresql";
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? builder.Configuration.GetConnectionString(
    dbProvider.ToLowerInvariant() == "sqlite" ? "SQLiteConnection" : "DefaultConnection");

// Configure DbContext based on the selected provider
if (dbProvider.ToLowerInvariant() == "sqlite")
{
    builder.Services.AddDbContext<SqliteOrderServiceContext>(options =>
        options.UseSqlite(connectionString, 
            sqliteOptions => sqliteOptions.MigrationsHistoryTable("__EFMigrationsHistory")));
    
    Console.WriteLine("Using SQLite database provider");
}
else
{
    builder.Services.AddDbContext<PostgresOrderServiceContext>(options =>
        options.UseNpgsql(connectionString,
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public")));
    
    Console.WriteLine("Using PostgreSQL database provider");
}

builder.Services.AddDiscoveryClient(builder.Configuration);

builder.Services.AddLogging(options =>
{
    options.AddSimpleConsole(c =>
    {
        c.TimestampFormat = "[yyyy-MM-ddTHH:mm:ss] ";
        c.UseUtcTimestamp = true;
        c.SingleLine = true;
    });
});

builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestMethod |
                      HttpLoggingFields.RequestPath |
                      HttpLoggingFields.RequestBody |
                      HttpLoggingFields.Duration |
                      HttpLoggingFields.ResponseStatusCode;
    o.CombineLogs = true;
});

// Disable HTTPS

builder.Services.Configure<IISOptions>(options =>
{
    options.AutomaticAuthentication = false; // Disable automatic authentication
});

// Build the app.
var app = builder.Build();

app.UseHttpLogging();
app.UseGraceterm();

PrepDB.prepPopulation(app);

app.UseSwagger();
app.UseSwaggerUI();



app.UseAuthorization();

app.MapControllers();

app.Run();
