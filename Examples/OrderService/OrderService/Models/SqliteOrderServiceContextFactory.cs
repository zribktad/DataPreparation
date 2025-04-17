using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderService.Models;

public class SqliteOrderServiceContextFactory : IDesignTimeDbContextFactory<SqliteOrderServiceContext>
{
    public SqliteOrderServiceContext CreateDbContext(string[] args)
    {
        
        var builder = new DbContextOptionsBuilder<OrderServiceContext>();
        
        // Aplikovanie SQLite konfigurácií
        builder.UseSqlite("DataSource=:memory:", sqliteOptions =>
        {
            // Nastavenie migračnej histórie
            sqliteOptions.MigrationsHistoryTable("__EFMigrationsHistory")
                .MigrationsAssembly("OrderService");
                
            // Nastavenie migračného assembly - nemusí byť potrebné, ak máte migrácie v rovnakom projekte
            // sqliteOptions.MigrationsAssembly(typeof(OrderServiceContext).Assembly.GetName().Name);
        });

        Console.WriteLine($"SQLite Design-Time Factory: Using  in memory connection ");

        return new SqliteOrderServiceContext(builder.Options);
    }
}