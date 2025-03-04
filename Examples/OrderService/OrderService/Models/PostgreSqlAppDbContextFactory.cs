using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderService.Models;

public class PostgreSqlOrderServiceContextFactory : IDesignTimeDbContextFactory<PostgresOrderServiceContext>
{
    public PostgresOrderServiceContext CreateDbContext(string[] args)
    {
        // Vytvorenie konfigurácie z appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var builder = new DbContextOptionsBuilder<OrderServiceContext>();
            
        // Získanie connection stringu pre PostgreSQL
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? 
                               configuration.GetConnectionString("DefaultConnection");
            
        // Aplikovanie PostgreSQL konfigurácií
        builder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            // Nastavenie migračnej histórie
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public").MigrationsAssembly("OrderService");
                
            // Nastavenie migračného assembly - nemusí byť potrebné, ak máte migrácie v rovnakom projekte
            // npgsqlOptions.MigrationsAssembly(typeof(OrderServiceContext).Assembly.GetName().Name);
        });

        Console.WriteLine($"PostgreSQL Design-Time Factory: Using connection string: {connectionString}");

        return new PostgresOrderServiceContext(builder.Options);
    }
}

