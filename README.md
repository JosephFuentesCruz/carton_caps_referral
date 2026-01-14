**Overview**
This repository implements the Carton Caps referral/deferred-deeplink service.
The README below explains prerequisites and exact commands for building, running,
testing, and inspecting the API (Swagger/OpenAPI) on Windows.

**Prerequisites**

- **.NET SDK**: Install .NET 8 SDK (or later). Verify with `dotnet --version`.
- **OS**: Windows (PowerShell commands shown below).
- **Editor**: Optional — Visual Studio, VS Code, or any text editor.

**Build**

- **Solution build**: From the repository root run:

```powershell
dotnet build carton_caps_referral.sln -c Debug
```

- **Project-specific build**: To build only the API project:

```powershell
dotnet build carton_caps_referral\\carton_caps_referral.csproj -c Debug
```

**Run (development)**

- The API uses Swagger when running in the `Development` environment and seeds sample data.
- Set the environment and run the API project from the repo root (PowerShell):

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project carton_caps_referral\\carton_caps_referral.csproj
```

- When started you'll see lines like "Now listening on: https://localhost:5001" in the console — open the URL and append `/swagger` to view the Swagger UI.
- If HTTPS is enabled and a browser warns about a certificate, accept/trust the dev certificate or use the HTTP URL shown in the console.

**Run (production-like)**

- Clear the environment variable (or set to `Production`) before running. In Production, seed+Swagger are disabled by the app.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Production'
dotnet run --project carton_caps_referral\\carton_caps_referral.csproj
```

**Tests**

- Run all tests (solution-level or tests project):

```powershell
dotnet test carton_caps_referral.Tests\\carton_caps_referral.Tests.csproj -c Debug
```

- To run tests with more verbose output:

```powershell
dotnet test --logger "console;verbosity=detailed"
```

- Run a single test or filter by name:

```powershell
dotnet test --filter "FullyQualifiedName~Namespace.ClassName.MethodName"
```

**OpenAPI / Swagger**

- The project exposes Swagger UI when `ASPNETCORE_ENVIRONMENT=Development`.
- After starting the app, open the base URL printed by Kestrel and visit `/swagger`.
- A generated OpenAPI YAML file is included at the repo root as [specification.yaml](specification.yaml).

**Key files and locations**

- **Program entry**: [carton_caps_referral/Program.cs](carton_caps_referral/Program.cs#L1-L120)
- **Controllers**: [carton_caps_referral/Controllers](carton_caps_referral/Controllers)
- **DTOs and contracts**: [carton_caps_referral/Contracts/DTOs](carton_caps_referral/Contracts/DTOs)
- **Tests**: [carton_caps_referral.Tests](carton_caps_referral.Tests)
- **OpenAPI spec**: [specification.yaml](specification.yaml)

**Common troubleshooting**

- "`dotnet` not found": ensure .NET SDK is installed and added to PATH. Run `dotnet --info`.
- Port conflicts: if Kestrel fails to start because the port is in use, change the port by setting `ASPNETCORE_URLS` before running, e.g. `$env:ASPNETCORE_URLS = 'http://localhost:5002'`.
- HTTPS dev cert: run `dotnet dev-certs https --trust` to trust the local development certificate.
- Tests failing locally: check which framework test project targets (some tests target net9.0 and net8.0); ensure an appropriate SDK is installed (having .NET 9 SDK is optional but may be required for tests targeting net9.0).

**Useful commands (quick reference)**

```powershell
# Build solution
dotnet build carton_caps_referral.sln

# Run API in Development (enables Swagger + seed data)
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project carton_caps_referral\\carton_caps_referral.csproj

# Run tests
dotnet test carton_caps_referral.Tests\\carton_caps_referral.Tests.csproj

# Trust dev cert (if needed for HTTPS)
dotnet dev-certs https --trust
```

If you want, I can also:

- add a `launch.json` / `tasks.json` for VS Code,
- or run the build & tests now and paste the output.
