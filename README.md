# Serilog.Enrichers.HttpContext [![Nuget](https://img.shields.io/nuget/v/Serilog.Enrichers.HttpContext.svg)](https://nuget.org/packages/Serilog.Enrichers.HttpContext/)

Enriches Serilog events with client IP, RequestBody, RequestQuery, HTTP request headers and memory usage. Correlation Id can be added via `WithRequestHeader("X-Correlation-Id", "CorrelationId")`.

**Supported frameworks:** .NET Standard 2.1, .NET 6, .NET 7, .NET 8, .NET 9, .NET 10

Install the _Serilog.Enrichers.HttpContext_ [NuGet package](https://www.nuget.org/packages/Serilog.Enrichers.HttpContext/)

```powershell
Install-Package Serilog.Enrichers.HttpContext
```
or
```shell
dotnet add package Serilog.Enrichers.HttpContext
```

Apply the enricher to your `LoggerConfiguration` in code:

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithClientIp()
    .Enrich.WithMemoryUsage()
    .Enrich.WithMemoryUsageExact()
    .Enrich.WithRequestBody()
    .Enrich.WithRequestQuery()
    .Enrich.WithRequestHeader("Header-Name1")
    // ...other configuration...
    .CreateLogger();
```

or in `appsettings.json` file (requires [Serilog.Settings.Configuration](https://www.nuget.org/packages/Serilog.Settings.Configuration/) or [Serilog.AspNetCore](https://www.nuget.org/packages/Serilog.AspNetCore/)):
```json
{
  "Serilog": {
    "MinimumLevel": "Verbose",
    "Using":  [ "Serilog.Enrichers.HttpContext" ],
    "Enrich": [
      "WithClientIp",
      "WithMemoryUsage",
      "WithMemoryUsageExact",
      "WithRequestBody",
      "WithRequestQuery",
      {
        "Name": "WithRequestHeader",
        "Args": {
          "headerName": "X-Correlation-Id",
          "propertyName": "CorrelationId"
        }
      },
      {
        "Name": "WithRequestHeader",
        "Args": { "headerName": "User-Agent" }
      }
    ],
    "WriteTo": [
      { "Name": "Console" }
    ]
  }
}
```

---

## Enricher Configuration

### WithClientIp

Adds the `ClientIp` property — client IP address. By default uses `"x-forwarded-for"` header. When behind a proxy, uses the configured header to determine the real IP. Returns `"unknown"` when the IP cannot be determined.

| Parameter    | Type   | Default             | Description                                 |
|--------------|--------|---------------------|---------------------------------------------|
| `headerName` | string | `"x-forwarded-for"` | Proxy header name containing the IP address |

**Code:**
```csharp
.Enrich.WithClientIp()
.Enrich.WithClientIp(headerName: "CF-Connecting-IP")
```

**appsettings.json:**
```json
"WithClientIp"
```
or with parameters:
```json
{
  "Name": "WithClientIp",
  "Args": { "headerName": "CF-Connecting-IP" }
}
```

Full example for custom proxy header:
```json
{
  "Serilog": {
    "MinimumLevel": "Verbose",
    "Using":  [ "Serilog.Enrichers.HttpContext" ],
    "Enrich": [
      {
        "Name": "WithClientIp",
        "Args": { "headerName": "CF-Connecting-IP" }
      }
    ],
    "WriteTo": [ { "Name": "Console" } ]
  }
}
```

### WithMemoryUsage

Adds the `MemoryUsage` property — process memory usage in bytes. Works in any context (does not require HTTP request).

**Code:**
```csharp
.Enrich.WithMemoryUsage()
```

**appsettings.json:**
```json
"WithMemoryUsage"
```

### WithMemoryUsageExact

Adds the `MemoryUsageExact` property — memory increase since the start of the request (in bytes).

**Required:** You must add `app.UseSerilogMemoryUsageExact()` middleware to the pipeline. Place it early, before request handlers (e.g. `MapGet`, `MapControllers`).

**Enricher registration**

Code:
```csharp
.Enrich.WithMemoryUsageExact()
```

appsettings.json:
```json
"WithMemoryUsageExact"
```

**Middleware registration**

`Program.cs` (minimal hosting):
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

var app = builder.Build();
app.UseSerilogMemoryUsageExact();
app.MapGet("/", () => "Hello");
app.Run();
```

`Startup.cs` (in `Configure` method):
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
{
    loggerFactory.AddSerilog();
    app.UseSerilogMemoryUsageExact();
    app.UseRouting();
    app.UseEndpoints(endpoints => { /* ... */ });
}
```

### WithRequestBody

Adds the `RequestBody` property — HTTP request body. **Note:** If controllers or model binding read the body before logging, add middleware at the start of the pipeline to enable buffering:

```csharp
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});
```

**Code:**
```csharp
.Enrich.WithRequestBody()
```

**appsettings.json:**
```json
"WithRequestBody"
```

### WithRequestQuery

Adds the `RequestQuery` property — HTTP request query string.

**Code:**
```csharp
.Enrich.WithRequestQuery()
```

**appsettings.json:**
```json
"WithRequestQuery"
```

### WithRequestHeader

You can use multiple `WithRequestHeader` to log different request headers. `WithRequestHeader` accepts two parameters: the first parameter `headerName` is the header name to log, and the second parameter `propertyName` is the log property name.

| Parameter      | Type   | Required | Description                                                         |
|----------------|--------|----------|---------------------------------------------------------------------|
| `headerName`   | string | yes      | HTTP header name                                                    |
| `propertyName` | string | no       | Log property name (if not specified — `headerName` without hyphens) |

When the header is absent, the property value is `null`.

**Code:**
```csharp
.Enrich.WithRequestHeader("User-Agent")
.Enrich.WithRequestHeader(headerName: "Content-Length", propertyName: "RequestLength")
.Enrich.WithRequestHeader("X-Correlation-Id", "CorrelationId")
```

**appsettings.json:**
```json
{
  "Name": "WithRequestHeader",
  "Args": { "headerName": "User-Agent" }
}
```
or with custom property name:
```json
{
  "Name": "WithRequestHeader",
  "Args": {
    "headerName": "X-Correlation-Id",
    "propertyName": "CorrelationId"
  }
}
```

Full example with multiple headers:
```json
{
  "Serilog": {
    "MinimumLevel": "Verbose",
    "Using": [ "Serilog.Enrichers.HttpContext" ],
    "Enrich": [
      { "Name": "WithRequestHeader", "Args": { "headerName": "User-Agent" } },
      { "Name": "WithRequestHeader", "Args": { "headerName": "header-name" } },
      {
        "Name": "WithRequestHeader",
        "Args": {
          "headerName": "Content-Length",
          "propertyName": "RequestLength"
        }
      }
    ],
    "WriteTo": [ { "Name": "Console" } ]
  }
}
```

#### Note

To include logged headers in `OutputTemplate`, the header name without `-` should be used if you haven't set the log property name. For example, if the header name is `User-Agent`, you should use `UserAgent`.

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.WithRequestHeader("User-Agent")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] {Level:u3} {UserAgent} {Message:lj}{NewLine}{Exception}")
```

---

## Installing into an ASP.NET Core Web Application

You need to register the `IHttpContextAccessor` singleton so the enrichers have access to the requests `HttpContext`. Without it, `WithClientIp`, `WithRequestBody`, `WithRequestQuery`, `WithRequestHeader`, and `WithMemoryUsageExact` will not add properties when logging outside a request context or when `HttpContext` is unavailable.

**If you use `WithMemoryUsageExact`**, you must add the `app.UseSerilogMemoryUsageExact()` middleware early in the pipeline (before request handlers) to capture memory at the start of each request.

This is what your `Startup` class should contain in order for this enricher to work as expected:

```cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MyWebApp
{
    public class Startup
    {
        public Startup()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] {Level:u3} Client IP: {ClientIp} Correlation Id: {CorrelationId} header-name: {headername} {Message:lj}{NewLine}{Exception}")
                .Enrich.WithClientIp()
                .Enrich.WithMemoryUsage()
                .Enrich.WithMemoryUsageExact()
                .Enrich.WithRequestBody()
                .Enrich.WithRequestQuery()
                .Enrich.WithRequestHeader("header-name")
                .Enrich.WithRequestHeader("another-header-name", "AnotherHeaderNameNewName")
                .CreateLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // ...
            services.AddHttpContextAccessor();
            // ...
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // ...
            loggerFactory.AddSerilog();
            app.UseSerilogMemoryUsageExact();  // Required for WithMemoryUsageExact enricher
            // ...
        }
    }
}
```

For minimal hosting (`Program.cs` with `WebApplication`):

```csharp
builder.Services.AddHttpContextAccessor();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

var app = builder.Build();
// ...
app.UseSerilogMemoryUsageExact();  // Required for WithMemoryUsageExact enricher
// ...
app.Run();
```
