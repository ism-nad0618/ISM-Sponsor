# ISM Sponsor Portal

ASP.NET Core 8.0 web application for managing sponsor relationships and Letters of Guarantee (LoGs).

## Setup

### Prerequisites
- .NET 8.0 SDK
- SQL Server (or SQL Server Express)

### Configuration

1. Copy `appsettings.json.example` to `appsettings.json`:
   ```bash
   cp appsettings.json.example appsettings.json
   ```

2. Update `appsettings.json` with your database connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost,1433;Database=ISMSponsor;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=true;MultipleActiveResultSets=true"
     }
   }
   ```

### Running the Application

1. Build the project:
   ```bash
   dotnet build
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open browser to `http://localhost:5000`

### Default Credentials

The database is seeded with the following users:

- **Admin**: username: `admin`, password: `admin`
- **Admissions**: username: `admissions`, password: `admissions`
- **Cashier**: username: `cashier`, password: `cashier`
- **Sponsor (ACME)**: username: `acme_sponsor`, password: `sponsor`
- **Sponsor (XYZ)**: username: `xyz_sponsor`, password: `sponsor`

⚠️ **Change these passwords in production!**

## Features

- School Year Management (CRUD operations)
- Student Management
- Sponsor Profile Management
- Letters of Guarantee (LoGs) tracking
- Role-based access control
- Reports

## Project Structure

```
Controllers/          # MVC Controllers
Data/                # Database context and initialization
Migrations/          # Entity Framework migrations
Models/              # Domain models and view models
Services/            # Business logic services
Views/               # Razor views
wwwroot/             # Static files (CSS, JS, images)
```

## License

Proprietary - International School Manila
# ISM-Sponsor
