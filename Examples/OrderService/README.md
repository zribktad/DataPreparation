# SWA Order Service

SWA Order service slouží pro správu objednávek a potřebných informací uživatele. Je součástí projektu https://gitlab.fel.cvut.cz/kimldavi/swa-main

## Technologie
SWA Order Service využívá  následující technologie:

- **ASP.NET Core 6.0**: Moderní webový framework od společnosti Microsoft, který poskytuje vysoký výkon a flexibilitu.
- **Entity Framework Core**: Výkonný ORM nástroj, který umožňuje jednoduchou práci s databází a zajišťuje efektivní správu dat.
- **Postgresql**: Spolehlivý a škálovatelný databázový systém, který zajišťuje bezpečné a rychlé ukládání a přístup k datům.
- **Docker**: Platforma pro kontejnerizaci, která umožňuje snadné nasazení a škálování aplikace v různých prostředích.

## Požadavky

- .NET SDK 8.0 nebo vyšší
- PostgreSQL
- Docker (pre kontejnerizaci)

## Instalace

1. Klonujte tento repozitár:
   ```bash
   git clone https://github.com/kimldavi/swa-orderservice.git
   cd swa-orderservice/OrderService
   ```

2. Nainstalujte závislosti:
   ```bash
   dotnet restore
   ```

## Konfigurace

- Upravte `appsettings.json` súbor podľa vašich potrieb. Typický súbor `appsettings.json` vyzerá nasledovne alebo cez enviromens variables:

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

## Migrace databáze

1. Vytvořte nebo aktualizujte schéma pomocí Entity Framework:
   ```bash
   dotnet ef database update
   ```

## Spuštění aplikace

- Pro spuštění samotné aplikace:
  ```bash
  dotnet run
  ```

- Pro spuštění pomocí Dockeru společně s databází:
  ```bash
  docker-compose up
  ```

## Používání

Základní návod na spuštění:

1. Spusťte aplikci podle návodu výše.
2. API bude přístupná na URL adrese, která je definovaná v konfiguraci (napr. `https://localhost:8080`).
3. Lze si prohlédnout jednotlivé body API pomocí Swaggeru na adrese `http://localhost:8080/swagger/index.html`






