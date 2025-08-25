# UMA-API: Domain-Driven Design Web API
### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with .NET extensions.  
- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) installed.  
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Azure Sql Server).  
- [Entity Framework Core CLI Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (install via dotnet tool install --global dotnet-ef) 

### Computer Specifications (Development Environment)
- Operating System: Windows 10/11, macOS or Linux (Windows 11 used in development)
- RAM: 8 GB or more recommended (32 GB used in development)
- Processor: Quad-core processor or better (Intel i5-1135G7 used in development)  
- Storage: At least 10 GB free disk space for SDKs, packages, and database files  (Local SqL Server and Azure Sql Server used in development) 
- IDE/Editor: Visual Studio 2022 or VS Code with .NET extensions  (Visual Studio 2022 used in development) 

### Setup Project
**1. Clone the Repository**

```bash
git clone https://github.com/marcus0813/UMA-API.git
```

**2. Naviate to the Project Directory**
   
```bash
cd your-repository-folder-name
```

**3. Restore Nuget Packages**

```bash
dotnet restore
```

**4. Update the Database**

Ensure your SQL Server instance is running and the connection string is configured in appsettings.Development.json. Then apply migrations with the command below:

```bash
dotnet ef database update --project UMA.Infrastructure --startup-project UMA.API
```

**5. Build and Run the Project**

```bash
dotnet run --project UMA.API
```

---
## Assumptions
This project is a **Web API built on .NET 7**, designed using **Domain-Driven Design (DDD)** and **Clean Architecture** principles. 

### Architecture Overview

**1. Architecture & Design**: Has a fundamental understanding of Domain-Driven Design and Clean Architecture principles.

The solution is organized into distinct layers that enforce DDD and Clean Architecture principles:

- **UMA.API (Presentation Layer)**  
  - Entry point of the application.  
  - Handles HTTP requests and responses.  
  - Contains controllers and API-specific configurations.  

- **UMA.Application (Application Layer)**  
  - Coordinates business logic.  
  - Uses services and queries to orchestrate use cases.  
  - Acts as an intermediary between Presentation and Domain layers.  

- **UMA.Domain (Domain Layer)**  
  - Core of the system.  
  - Calls domain entities to perform business logic.
  - Persists changes via repositories.  

- **UMA.Infrastructure (Infrastructure Layer)**  
  - Implements interfaces defined in Application layer.  
  - Provides persistence (Entity Framework Core), external services, file systems, and database connectivity.
    
- **UMA.Shared (Shared Layer)** 
  - Contains cross-cutting objects and that are reused across multiple layers.
    - Common Dtos: Standardized objects used.
    - Request Dtos: Request/response contracts used in API and Application layers.
    
- **UMA.UnitTests (Testing Layer)**  
  - Unit tests for application layer.  
  - Ensures correctness and stability of the system across layers.

**2. Centralized Packages**: NuGet packages are managed in a centralized way using a Directory.Packages.props file in the solution directory.

**3. Authentication & Authorization**: Understands the project's use of JWT authentication with both access and refresh tokens, and the implementation of secure, user-specific endpoints.

**4. Database**: Using Entity Framework Core to manage database migrations, and also integrating with both Local SQL Server and Azure SQL server.

**5. External Services**: Access and configured credentials for an Azure Blob Storage account are available for file uploads.

**6. Error & Logging**: Familiar with the project's approach to structured error handling and the use of Serilog for logging.

**7. Testing**: It's assumed that the developer will follow existing patterns for unit testing the application layer.

**8. Security**: Aware of the use of bcrypt for password hashing.

**9. Documentation**: Familiar with and will follow existing documentation practices for C# methods.
