# Order Service

 Order service is used for managing orders and necessary user information. 

## Technologies
Order Service uses the following technologies:

- **ASP.NET Core 6.0**: Modern web framework from Microsoft that provides high performance and flexibility.
- **Entity Framework Core**: Powerful ORM tool that allows simple database operations and ensures efficient data management.
- **PostgreSQL**: Reliable and scalable database system that ensures secure and fast data storage and access.
- **Docker**: Containerization platform that enables easy deployment and scaling of applications in various environments.

## Requirements

- .NET SDK 8.0 or higher
- PostgreSQL
- Docker (for containerization)

## Configuration

- Modify the `appsettings.json` file according to your needs. A typical `appsettings.json` file looks like this or can be set via environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=OrderService;Username=ear;Password=ear"
  },
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware": "Information"
    }
  },
  "AllowedHosts": "*",
  "eureka": {
    "client": {
      "shouldRegisterWithEureka": true,
      "serviceUrl": "http://eureka:8761/eureka/"
    },
    "instance": {
      "appName": "order-service",
      "port": 8080
    }
  }
}
```

## Database Migration

1. Create or update the schema using Entity Framework:
   ```bash
   dotnet ef database update
   ```

## Running the Application

- To run the application itself:
  ```bash
  dotnet run
  ```

- To run using Docker along with the database:
  ```bash
  docker-compose up
  ```

## Usage

Basic startup guide:

1. Run the application according to the instructions above.
2. The API will be accessible at the URL defined in the configuration (e.g., `https://localhost:8080`).
3. You can view the API endpoints using Swagger at `http://localhost:8080/swagger/index.html`






