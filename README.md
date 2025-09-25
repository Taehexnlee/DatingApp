# DatingApp

Full-stack dating platform built with an ASP.NET Core 8 REST API and an Angular 19 single-page application. It ships with realtime chat using SignalR, profile photos via Cloudinary, and role-based administration powered by ASP.NET Identity.

## Project Structure
- `API/` – ASP.NET Core backend, Entity Framework Core data layer, SignalR hubs
- `client/` – Angular frontend, Bootstrap/Bootswatch UI, shared services
- `docker-compose.yml` – Optional SQL Server Edge container for local development

## Features
- JWT-based authentication with ASP.NET Identity roles (Member, Moderator, Admin)
- User profiles with photo uploads stored in Cloudinary
- Like system and realtime presence-aware messaging via SignalR hubs
- Admin dashboard endpoints for user moderation
- Database seeding from `API/Data/UserSeedData.json` for quick local setup

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20 LTS](https://nodejs.org/) and npm
- [Angular CLI](https://angular.dev/tools/cli) (`npm install -g @angular/cli`) – optional but recommended
- [Docker](https://www.docker.com/) (or a local SQL Server instance)
- Cloudinary account (for production photo uploads)

## Getting Started
### 1. Boot the database
```
docker compose up -d
```
The API is pre-configured to connect to `localhost:1433` using the connection string in `API/appsettings.Development.json`. Update the value or provide your own SQL Server instance if needed.

### 2. Configure secrets
Create or edit `API/appsettings.Development.json` with values for:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=DatingDB;User Id=SA;Password=Password@1;TrustServerCertificate=True"
  },
  "TokenKey": "your super secret key",
  "CloudinarySetting": {
    "CloudName": "<cloud-name>",
    "ApiKey": "<api-key>",
    "ApiSecret": "<api-secret>"
  }
}
```
Avoid committing real credentials—use user secrets, environment variables, or container secrets outside of source control for non-development environments.

### 3. Run the API
```
cd API
dotnet restore
dotnet watch run
```
The service listens on `https://localhost:5001` (and `http://localhost:5000`). On first start it applies migrations, clears stale SignalR connection rows, and seeds users/roles.

### 4. Run the Angular client
```
cd client
npm install
npm start
```
The SPA loads at `http://localhost:4200` and is configured to call the API at `https://localhost:5001`.

## Seed Users & Test Accounts
User data comes from `API/Data/UserSeedData.json`. Every seeded account uses the password `Pa$$w0rd`. An `admin` user is created and assigned both `Admin` and `Moderator` roles for managing members and photos.

## Useful Commands
- `dotnet ef migrations add <Name> -p API` – create a new EF Core migration
- `dotnet ef database update -p API` – apply migrations manually
- `npm run build` (inside `client/`) – production build of the Angular app
- `ng test` – run Angular unit tests (when added)

## Troubleshooting
- CORS: the API allows `http://localhost:4200`. Update `API/Program.cs` if you serve the client from a different origin.
- HTTPS dev certificates: ensure the .NET dev certificate is trusted (`dotnet dev-certs https --trust`).
- SignalR: for local testing behind proxies, verify WebSockets are enabled and that `AllowCredentials` matches your client origin.

## Next Steps
- Replace the sample Cloudinary keys with environment-specific secrets
- Add CI checks or additional testing as needed (`.github/workflows/master_dating-app-course.yml` contains a starter pipeline)
- Configure deployment targets for both API and client applications
