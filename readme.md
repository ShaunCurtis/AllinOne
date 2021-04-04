# Blazor AllinOne 

This article shows how to build a single Blazor application that runs in both WASM and Server Modes.

![screenshot](https://shauncurtis.github.io/siteimages/Articles/AllinOne/Screenshot.png)

## Code Repository

The Code repository for the article is [here - https://github.com/ShaunCurtis/AllinOne ](https://github.com/ShaunCurtis/AllinOne)

## The Solution and Projects

Create a new solution called **Blazor** using the Blazor WebAssembly template.  Don't choose to host it on Aspnetcore.  You will get a single project called **Blazor**.

Now add a second project to the solution using the *ASP.NET Core Web App* template.  Call it **Blazor.Web**.  Set it as the startup project.

The solution should now look like this:

![Solution](https://shauncurtis.github.io/siteimages/Articles/AllinOne/Base-Projects.png)

## Blazor Project Changes

The solution runs the WASM context in a sub-directory on the web site.  To get this working there are a few modifications that need to be made to the *Blazor* project.

1. Move the contents of wwwroot to *Blazor.Web* and delete everything in *wwwroot*.
2. Add a `StaticWebAssetBasePath` entry to the project file set to `wasm`.  This is case sensitive in the context in which it is used, so stick to small letters.  
3. Add the necessary packages.

The project file should look like this:

```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">

  <PropertyGroup>
    <StaticWebAssetBasePath>wasm</StaticWebAssetBasePath>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="5.0.4" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="5.0.4" PrivateAssets="all" />
    <PackageReference Include="System.Net.Http.Json" Version="5.0.0" />
  </ItemGroup>

  <ItemGroup>
    <Folder Include="wwwroot\" />
  </ItemGroup>

</Project>
```

### MainLayout

`MainLayout` needs to be modified to handle both contexts.  The solution changes the colour scheme for each context.  WASM  *Teal* and Server *Steel*. 

```csharp
@inherits LayoutComponentBase
<div class="page">
    @*change class*@
    <div class="@_sidebarCss">
        <NavMenu />
    </div>
    <div class="main">
        <div class="top-row px-4">
            <a href="https://docs.microsoft.com/aspnet/" target="_blank">About</a>
        </div>
        <div class="content px-4">
            @Body
        </div>
    </div>
</div>

@code {
    [Inject] NavigationManager NavManager { get; set; }
    private bool _isWasm => NavManager?.Uri.Contains("wasm", StringComparison.CurrentCultureIgnoreCase) ?? false;
    private string _sidebarCss => _isWasm ? "sidebar sidebar-teal" : "sidebar sidebar-steel";
}
```

Add the following Css styles to the component Css file below `.sidebar`.
```css
.sidebar {
    background-image: linear-gradient(180deg, rgb(5, 39, 103) 0%, #3a0647 70%);
}

/* Added Styles*/
.sidebar-teal {
    background-image: linear-gradient(180deg, rgb(0, 64, 128) 0%, rgb(0,96,192) 70%);
}

.sidebar-steel {
    background-image: linear-gradient(180deg, #2a3f4f 0%, #446680 70%);
}
/* End Added Styles*/
```

### NavMenu

Add code and markup - it adds a link to switch between contexts.

```csharp
<div class="top-row pl-4 navbar navbar-dark">
    @*Change title*@
    <a class="navbar-brand" href="">Blazor</a>
    <button class="navbar-toggler" @onclick="ToggleNavMenu">
        <span class="navbar-toggler-icon"></span>
    </button>
</div>

<div class="@NavMenuCssClass" @onclick="ToggleNavMenu">
    <ul class="nav flex-column">
        @*Add links between contexts*@
        <li class="nav-item px-3">
                <NavLink class="nav-link" href="@_otherContextUrl" Match="NavLinkMatch.All">
                    <span class="oi oi-home" aria-hidden="true"></span> @_otherContextLinkName
                </NavLink>
        </li>
        <li class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="oi oi-home" aria-hidden="true"></span> Home
            </NavLink>
        </li>
        <li class="nav-item px-3">
            <NavLink class="nav-link" href="counter">
                <span class="oi oi-plus" aria-hidden="true"></span> Counter
            </NavLink>
        </li>
        <li class="nav-item px-3">
            <NavLink class="nav-link" href="fetchdata">
                <span class="oi oi-list-rich" aria-hidden="true"></span> Fetch data
            </NavLink>
        </li>
    </ul>
</div>

@code {
    [Inject] NavigationManager NavManager { get; set; }
    private bool _isWasm => NavManager?.Uri.Contains("wasm", StringComparison.CurrentCultureIgnoreCase) ?? false;
    private string _otherContextUrl => _isWasm ? "/" : "/wasm";
    private string _otherContextLinkName => _isWasm ? "Server Home" : "WASM Home";
    private string _title => _isWasm ? "AllinOne WASM" : "AllinOne Server";
    private bool collapseNavMenu = true;
    private string NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }
}
```
### FetchData.razor

Update the Url for getting forecasts by adding a `/` at the start, the file is now in the root and not in `wasm`.

```csharp
protected override async Task OnInitializedAsync()
{
    forecasts = await Http.GetFromJsonAsync<WeatherForecast[]>("/sample-data/weather.json");
}
```

## Blazor.Web

Update the project file:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Server" Version="5.0.3" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Blazor\Blazor.csproj" />
  </ItemGroup>
</Project>
```

Add a Razor Page to *Pages* called *WASM.cshtml* - the launch page for the WASM SPA.

```html
@page "/wasm"
@{
    Layout = null;
}

<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <title>Blazor</title>
    @*Change base*@
    <base href="/wasm/" />
    @*Update Link hrefs*@
    <link href="/css/bootstrap/bootstrap.min.css" rel="stylesheet" />
    <link href="/css/app.css" rel="stylesheet" />
    <link href="/wasm/Blazor.styles.css" rel="stylesheet" />
</head>
<body>
    <div id="app">Loading...</div>
    <div id="blazor-error-ui">
        An unhandled error has occurred.
        <a href="" class="reload">Reload</a>
        <a class="dismiss">🗙</a>
    </div>
    @*Update js sources *@
    <script src="/wasm/_framework/blazor.webassembly.js"></script>
</body>
</html>
```
Add a second Razor Page to *Pages* called *Server.cshtml* - the launch page for the Servr SPA.

```csharp
@page "/"
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@{
    Layout = null;
}

<!DOCTYPE html>
<html>

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <title>Blazor</title>
    <base href="/" />
    <link href="/css/bootstrap/bootstrap.min.css" rel="stylesheet" />
    <link href="/css/site.css" rel="stylesheet" />
    <link href="/wasm/Blazor.styles.css" rel="stylesheet" />
</head>

<body>
    <component type="typeof(Blazor.App)" render-mode="ServerPrerendered" />

    <div id="blazor-error-ui">
        <environment include="Staging,Production">
            An error has occurred. This application may no longer respond until reloaded.
        </environment>
        <environment include="Development">
            An unhandled exception has occurred. See browser dev tools for details.
        </environment>
        <a href="" class="reload">Reload</a>
        <a class="dismiss">🗙</a>
    </div>

    <script src="_framework/blazor.server.js"></script>
</body>
</html>
``` 

### Index.cshtml

Update the `@page` directive to `@page "/index"`.

### Startup.cs

Update `Startup` to handle WASM and Server middleware paths.

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorPages();
        services.AddServerSideBlazor();

        // Server Side Blazor doesn't register HttpClient by default
        // Thanks to Robin Sue - Suchiman https://github.com/Suchiman/BlazorDualMode
        if (!services.Any(x => x.ServiceType == typeof(HttpClient)))
        {
            // Setup HttpClient for server side in a client side compatible fashion
            services.AddScoped<HttpClient>(s =>
            {
                // Creating the URI helper needs to wait until the JS Runtime is initialized, so defer it.
                var uriHelper = s.GetRequiredService<NavigationManager>();
                return new HttpClient
                {
                    BaseAddress = new Uri(uriHelper.BaseUri)
                };
            });
        }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/wasm"), app1 =>
        {
            app1.UseBlazorFrameworkFiles("/wasm");
            app1.UseRouting();
            app1.UseEndpoints(endpoints =>
            {
                endpoints.MapFallbackToPage("/wasm/{*path:nonfile}", "/wasm");
            });
        });

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapBlazorHub();
            endpoints.MapRazorPages();
            endpoints.MapFallbackToPage("/Server");
        });
    }
}
```

## Run the Application

The application should now run.  It will start in the Server context.  Switch to the WASM context via the link in the left menu.  You should see the the colour change as you switch between contexts.

## Adding a DataService

While the above configuation works, it needs some demo code to show how it handles more conventional data services.  We'll modify the solution to work with a very basic data services to show the DI and interface concepts that should be used.

Add *Data* and *Services* folders to the *Blazor* project.

### WeatherForecast.cs

Add a `WeatherForecast` class to *Data*.

```csharp
public class WeatherForecast
{
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public string Summary { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
```

### IWeatherForecastService.cs

Add a `IWeatherForecastService` interface to *Services*.

```csharp
    public interface IWeatherForecastService
    {
        public Task<List<WeatherForecast>> GetRecordsAsync();
    }
```

### WeatherForecastServerService.cs

Add a `WeatherForecastServerService` class to *Services*.  Normally this would interface to a database, but here we're just creating a set of dummy records.

```csharp
public class WeatherForecastServerService : IWeatherForecastService
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private List<WeatherForecast> records = new List<WeatherForecast>();

    public WeatherForecastServerService()
        => this.GetForecasts();

    public void GetForecasts()
    {
        var rng = new Random();
        records = Enumerable.Range(1, 10).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = rng.Next(-20, 55),
            Summary = Summaries[rng.Next(Summaries.Length)]
        }).ToList();
    }

    public Task<List<WeatherForecast>> GetRecordsAsync()
        => Task.FromResult(this.records);
}
```

### WeatherForecastAPIService.cs

Add a `WeatherForecastAPIService` class to *Services*.

```csharp
public class WeatherForecastAPIService : IWeatherForecastService
{
    protected HttpClient HttpClient { get; set; }

    public WeatherForecastAPIService(HttpClient httpClient)
        => this.HttpClient = httpClient;

    public async Task<List<WeatherForecast>> GetRecordsAsync()
        => await this.HttpClient.GetFromJsonAsync<List<WeatherForecast>>($"/api/weatherforecast/list");
}
```

### WeatherForecastController.cs

Finally add a `WeatherForecastController` class to the *Blazor.Web* project in a *Controller* folder.

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazor.Data;
using Microsoft.AspNetCore.Mvc;
using MVC = Microsoft.AspNetCore.Mvc;
using Blazor.Services;

namespace Blazor.Web.APIControllers
{
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        protected IWeatherForecastService DataService { get; set; }

        public WeatherForecastController(IWeatherForecastService dataService)
            => this.DataService = dataService;

        [MVC.Route("/api/weatherforecast/list")]
        [HttpGet]
        public async Task<List<WeatherForecast>> GetList() => await DataService.GetRecordsAsync();
    }
}
```

### Blazor Project Program.cs

Add the API service to *Program.cs* in the *Blazor* project, declaring it through it's `IWeatherForecastService`.

```csharp
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddScoped<IWeatherForecastService, WeatherForecastAPIService>();

        await builder.Build().RunAsync();
    }
}
```

### Blazor.Web Startup.cs

Add the server service to *Startup.cs* in the *Blazor.Web* project, again through it's `IWeatherForecastService`.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddRazorPages();
    services.AddServerSideBlazor();
    services.AddScoped<IWeatherForecastService, WeatherForecastServerService>();
    .....
}
```

## Building and Run the project

The solution should now build and run.

![Blazor Project](https://shauncurtis.github.io/siteimages/Articles/AllinOne/Blazor-Project.png)

![Blazor.Web Project](https://shauncurtis.github.io/siteimages/Articles/AllinOne/Blazor-Web-Project.png)

## How Does It Work?

Fundimentally, the difference between a Blazor Server and a Blazor WASM Application is the context in which it's run.  In the solution all SPA code is built in the Web Assembly project, and used by both the WASM and Server contexts.  There's no "shared" code library code, because it's exactly the same front end code with the same entrypoint - App.razor.  The different between the two contexts, is the provider of the backend services.

The web assembly project is declared `<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">`.  It builds both a standard *Blazor.dll* file and the WASM specific code including the Web Assembly "boot configuration file" *blazor.boot.json*.

In the web assembly context, the initial page loads *blazor.webassembly.js*.  This loads *blazor.boot.json* which tells *blazor.webassembly.js* how to "boot" the Web assembly code in the browser.  It runs `Program` which builds the `WebAssemblyHost`, loads the defined services, and starts the `Renderer` which replaces the *app* html element with the root component specified in `Program`.  This loads the router, which reads the Url, gets the appropiate component, loads it into the specified layout, and begins the rendering process.  SPA up and running.

In the Server context, the server side code picks up the component reference in the initial load page and statically renders it.  It passes the rendered page to the client.  This loads and runs `blazor.server.js`, which calls back to the server SignalR Hub and gets the dynamically rendered app root component.  SPA up and running.  The services container and renderer are in the Blazor Hub - started by calling `services.AddServerSideBlazor()` in Startup when the web server starts.

The data services we implemented demonstrate Dependancy injection and interfaces.  The UI components - in our case `FetchData` consume the `IWeatherForcastService` service registered in Services.  In the WASM context, the services container starts `WeatherForecastAPIService`, while in the Server context, the services container starts `WeatherForecastServerService`.  Two different services, conforming to the same interface and consumed by the UI components using the interface.  The UI components don't care which service they consume, it just needs to implement `IWeatherForcastService`.

## Wrap Up

Hopefully this article has provided an insight into how Blazor SPAs work and the real differences between a Server and WASM Blazor SPA.

If you are reading this well into the future, the most recent version of this article will be [here](https://shauncurtis.github.io/articles/Blazor-AllinOne.html).
