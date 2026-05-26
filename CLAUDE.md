# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Restore and build
dotnet restore GrandNode.sln
dotnet build GrandNode.sln

# Run the web app
dotnet run --project src/Web/Grand.Web

# Run all tests (requires MongoDB on port 27017)
dotnet test GrandNode.sln

# Run a single test project
dotnet test src/Tests/Grand.Business.Catalog.Tests/Grand.Business.Catalog.Tests.csproj

# Format code
dotnet format GrandNode.sln
```

MongoDB must be running for tests: `docker run -d -p 27017:27017 mongo`

.NET Aspire workload is required: `dotnet workload install aspire`

## Architecture

GrandNode2 is a multi-store, multi-vendor, multi-tenant e-commerce platform on .NET 10 + MongoDB.

### Layer Structure

```
src/
├── Core/
│   ├── Grand.Domain          # Domain entities and aggregates (Product, Order, Customer…)
│   ├── Grand.Data            # IRepository<T> abstraction; MongoDB and LiteDB implementations
│   ├── Grand.Infrastructure  # Plugin/module loader, startup wiring, domain events
│   ├── Grand.SharedKernel    # Shared interfaces, attributes, exceptions
│   └── Grand.Mapping         # AutoMapper profiles
├── Business/
│   ├── Grand.Business.Core   # CQRS commands, domain events, service interfaces
│   └── Grand.Business.*      # Service implementations by domain (Catalog, Checkout, Customers…)
├── Web/
│   ├── Grand.Web             # Main ASP.NET Core host
│   ├── Grand.Web.Admin       # Admin area (Razor views)
│   ├── Grand.Web.Store       # Storefront (Razor views)
│   ├── Grand.Web.Vendor      # Vendor portal
│   └── Grand.Web.Common      # Shared web utilities, base controllers, validators
├── Modules/                  # Optional feature modules (Api, Installer, Migration, ScheduledTasks)
├── Plugins/                  # Payment, shipping, tax, auth, widget plugins
└── Tests/                    # MSTest unit tests mirroring Business/* structure
```

### Key Patterns

**CQRS via MediatR** — all business operations go through MediatR. Commands live in `Grand.Business.Core.Commands`; handlers implement `IRequestHandler<TCommand, TResult>`. Domain events use `INotification` + `INotificationHandler`.

**Repository pattern** — `IRepository<T>` in `Grand.Data` abstracts MongoDB/LiteDB. Services receive repositories via DI; never access the database directly from controllers.

**Service layer** — interfaces declared in `Grand.Business.Core.Interfaces.*`, implemented in matching `Grand.Business.*` projects. Registered in each project's `StartupApplication : IStartupApplication`.

**Plugin system** — plugins in `src/Plugins/` are shadow-copied to `Grand.Web/Plugins/` and dynamically loaded. Each plugin implements `IPlugin` (install/uninstall lifecycle) and `IStartupApplication` (DI + middleware registration). Decorated with `[PluginInfo]` assembly attribute.

**Module system** — modules in `src/Modules/` are feature-flagged via `FeatureManagement` config and loaded with isolated `AssemblyLoadContext`.

**Validation** — FluentValidation; validators are registered automatically and run in the MediatR pipeline.

**Caching** — `ICacheBase` abstraction; Redis for distributed deployments, in-memory for single-server.

### DI Registration Convention

Every project exposes a class `StartupApplication : IStartupApplication` that calls `services.AddScoped/AddTransient` for its own services. Scrutor is used for assembly scanning where bulk registration makes sense. Do not register services from one layer directly in another layer's startup.

### Central Package Management

All NuGet versions are defined in `Directory.Packages.props` at the root. Do not add `Version="…"` to individual `.csproj` files — add the version entry to `Directory.Packages.props` instead.

### Code Style

`.editorconfig` enforces Allman braces, `var` for evident types, no `this.` qualification, and 4-space indentation. Run `dotnet format` before committing.
