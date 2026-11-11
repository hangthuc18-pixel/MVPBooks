# MVPBooks

Mini Value Project: ASP.NET Core MVC application for CRUD operations on one `Books` table.

## Technology
- .NET 10 / ASP.NET Core MVC
- Entity Framework Core 10
- SQL Server
- Bootstrap 5

## Run locally
1. Install the .NET 10 SDK and SQL Server LocalDB.
2. Open a terminal in the `MVPBooks` project folder.
3. Restore packages: `dotnet restore`
4. Apply migrations: `dotnet ef database update`
5. Run: `dotnet run --launch-profile http`
6. Open the URL printed by the terminal (the current HTTP profile uses `http://localhost:5106`).

The local connection string is in `appsettings.json` and uses `(localdb)\MSSQLLocalDB`. LocalDB is for development only; it is not available to Azure App Service.

## Azure configuration
For Azure App Service, provision a hosted SQL Server database (for example Azure SQL Database) and configure the App Service application setting:

- Name: `ConnectionStrings__DefaultConnection`
- Value: the connection string for the hosted database.

Do not put production credentials in `appsettings.json` or commit them to Git. Apply EF Core migrations to the hosted database as a controlled deployment step before directing users to the site.

## GitHub Actions
The workflow in `.github/workflows/azure-deploy.yml` restores, builds and publishes the app on pushes to `main`. It deploys only when both repository variable `AZURE_WEBAPP_NAME` and repository secret `AZURE_WEBAPP_PUBLISH_PROFILE` are configured. Create the App Service first, then add these values in GitHub repository Settings. Never commit the publish profile.

## Current MVP scope
The current deliverable intentionally focuses on the instructor's mini MVP requirement: CRUD for one table (`Books`) and deployment with CI/CD. Borrower management, loans/returns, authentication and role management are future extensions, not included in this one-table MVP.
