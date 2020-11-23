# Building a Site that Delivers Blazor Server and WASM

## Overview

There are significant challenges to creating a combined Blazor WASM and Server application in the same solution, and running on the same web site.  This article examines those challenges, and shows how they can be overcome.

## The Challenges

### Routing

The biggest, but not the most obvious is routing. If I go to https://mysite/weatherforecast/1 what do you do? Open it in WASM or Server?  You could use https://mysite/wasm/weatherforecast/1 but that's clunky, with different routing paths to hanlde different scenarios.  You quickly start trying to jump tho' hoops backwards.  So, what does routing actually do?

1. You typically click on an `<a>` or call `NavigationManager` directly.  All paths lead to the Navigation Manager raising a navigation event.
2. The Router is wired into the navigation event and gets called.
3. It looks up the route in its routing table and finds the associated `IComponent` class.
4. It re-renders itself (it's a component at the base of the RenderTree) using the *layout* specified, passing the layout the new component.

### Shared Code Base

The two applications need to share the same code base.  We don't want two copies of `App.razor`, `Index.razor`,...

### A Single Web Site that Switches between Applications

How do we structure a single website, with paths and access to shared functionality?  How do we separate loading of each application?

## Code and Examples

The Repo is [here at Github](https://github.com/ShaunCurtis/AllinOne).

You can view the site on Azure [here at https://allinoneserver.azurewebsites.net/](https://allinoneserver.azurewebsites.net/)

## The Solution

The solution is built using standard issue Visual Studio Community Edition.  It's DotNetCore 3.1.  The starting point is a out-of-the-box solution created with the Blazor WASM template with ASP.NET Core hosted set.  You get three projects - Client, Server, Shared - in your solution.  Run it before making any changes to make sure it works!

We're going to re-structure the solution so:

- *Client* holds the minimum code to build the WASM application.
- *Server* holds the minimum code to build the Server application, and run the web site.
- *Shared* holds almost all the C# code - everything we can share.

Your starting point looks like this.

![client](/Images/Starting-Solution.png)

## Shared Project

Open the Shared Project configuration files (double-click on the project title in Solution Explorer).

1. Set the project Sdk to `Microsoft.NET.Sdk.Razor` and the language version.  This ensures Visual Studio handles razor files correctly.
2. Add the necessary packages.  You can either use Nuget or just copy and paste into the file.  Visual Studio will install them when you save the project file.

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <RazorLangVersion>3.0</RazorLangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="3.2.1" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="3.2.1" PrivateAssets="all" />
    <PackageReference Include="Microsoft.AspNetCore.WebUtilities" Version="2.2.0" />
    <PackageReference Include="System.Net.Http.Json" Version="3.2.0" />
  </ItemGroup>

</Project>
```

## Project Restructuring

### Shared

Create the following folder structure in *Shared*.

![Shared Folder Structure](\../../Images/shared-folders.png)

Move or create the following files in Controls:
(all classes should be in the *solution*.Shared.Component namespace)

- MainLayout.razor (move from Client)
- NavMenu.razor (move from Client)
- Server.razor (create as new Razor Component)
- ServerLayout.razor (create as new Razor Component)
- SurveyPrompt.razor (move from Client)
- ViewLink.cs (create as new class)
- WASM.razor (create as new Razor Component)
- WASMLayout.razor (create as new Razor Component)

Create the following files in ViewManager:

> [Link to Repo](https://github.com/ShaunCurtis/AllinOne/tree/master/All-In-One/Shared/Components/ViewManager)

- IView.cs (New or from the Repo)
- ViewBase.cs (New or from the Repo)
- ViewData.cs (New or from the Repo)
- ViewManager.cs (New or from the Repo)

Move or create the following files in Views:

- Index.razor (copy from Client)
- Counter.razor (copy from Client)
- FetchData.razor (copy from Client)

Move or create the following files in Data:
(all classes should be in the *solution*.Shared.Data namespace)

- IWeatherForecastService.cs (create as new interface)
- WeatherForecast.cs (move from root)
- WeatherForecastService.cs (create as new class)
- WeatherForecastWASMService.cs (create as new class)

Your Shared project should now look like this:

![shared files](/Images/shared-files.png)

### Client

- Move the *wwwroot* folder to the Server project.
- Delete *App.razor*
- Delete *_imports.razor*
- Delete *Pages*

Your Client project should now look like this (a program class):

![client project](/Images/Client-Project.png)

### Server

- Rename *_Layout.cshtml* to *_Host.cshtml* and move it to *Pages* 
- Delete *Shared*
- Rename *wwwroot/index.html* to *wasm.html*.

Your Server project should now look like this (a program class):

![server project](/Images/Server-Project.png)


## Data and Services

The first challenge is how to handle data access in `Fetchdata`.  The Server version just makes it up, the WASM version makes an API call. The API code makes it up.  We need a standardised interface for `Fetchdata`.

We solve this by using Interface dependency injection:
- defining an interface for the data service `IWeatherForecastService` that `Fetchdata` uses to access data.
- Loading `IWeatherForecastService` interface service in the Service Containers for the WASM and Server applications.

`Fetchdata` defines an injected property of type `IWeatherForecastService`.  The application injects whichever `IWeatherForecastService` is configured, `WeatherForecastService` in Server and `IWeatherForecastWASMService` in WASM.

This may be overcomplicated for here, but the purpose of this article is to solve the problems, not step round them.

### WeatherForecast.cs

```c#
// sort the namespace
namespace AllinOne.Shared.Data
```

### IWeatherForecastService.cs

Replace the template code with:

```c#
using System.Threading.Tasks;

namespace AllinOne.Shared.Data
{
    public interface IWeatherForecastService
    {
        public Task<WeatherForecast[]> GetForecastAsync();
    }
}
```

### WeatherForecastService

Replace the template code with:

```c#
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AllinOne.Shared.Data
{
    // Implements IWeatherForecastService
    public class WeatherForecastService : IWeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private WeatherForecast[] Forecasts;

        // now create the dataset on startup
        public WeatherForecastService()
        {
            var rng = new Random();
            var startDate = DateTime.Now.AddDays(-14);
            Forecasts =  Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToArray();
        }

        // IWeatherForecastService implementation return the dataset
        public virtual Task<WeatherForecast[]> GetForecastAsync() => Task.FromResult(Forecasts);

    }
}
```

### WeatherForecastWASMService.cs

Replace the template code with:

```c#
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AllinOne.Shared.Data
{
    // Inherits from WeatherForecastService so we don't have to do anything twice
    public class WeatherForecastWASMService : WeatherForecastService
    {
        private HttpClient Http { get; set; } = null;

        // HttpClient gets injected at startup
        public WeatherForecastWASMService( HttpClient http) => Http = http;

        // IWeatherForecastService implementation calls the API on the server
        public async override Task<WeatherForecast[]> GetForecastAsync() => await Http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
    }
}
```

Update the `WeatherForecastController` in the Server project.  It becomes an "interface" to the server side data service.

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
// add reference
using AllinOne.Shared.Data;

namespace AllinOne.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> logger;
        
        // add data service
        private IWeatherForecastService DataService { get; set; }

        //  Capture dataservice at initialisation 
        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastService dataService)
        {
            this.DataService = dataService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            // use the dataservice to get the data set - in this case the Server data service
            return await DataService.GetForecastAsync();
        }
    }
}
```

### View Manager

Moving on to the routing challenge.  The solution implemented here below bypasses the journey through `NavigationManager` and `Router`.  A new component `ViewManager` handles View management.  It replaces Router directly.  Components call the ViewManager directly to change the displayed View component.

The full class code set isn't shown here - it's rather long.

The classes/interfaces are:
- `IView` interface.  All Views need to implement this interface.
- `ViewBase` is the base class for all Views.  It implements `IView`.
- `ViewData` is a configuration class for `ViewManager`.
- `ViewManager`.  
  
`ViewManager`:

1. Loads the view into a standard `Layout` in the same way as Router.
2. Exposes `LoadViewAsync` as the main View loading method. There are various incarnations to handle different ways of passing view data.
3. Cascades itself, so all components in the RenderTree have access to the running instance and can call `LoadViewAsync`.
4. Maintains a history of views rendered.
5. Can read data from a querystring.

Copy or lift all the files in the *ViewManager* directory in the GitHub Repo.  Change the root namespace on the files to match your project name if you haven't used *AllinOne*.

Files
- IView.cs
- ViewBase.cs
- ViewData.cs
- ViewManager.cs

### Views

The old "Pages" become views. We:
- Set to inherit from `ViewBase` and the namespace.
- Include all assembly references - we're now in a library environment with no `_Imports.razor`.
- Remove all the `@page` directives.  They're obselete.

*Counter.razor* & *Index.razor*

```c#
//  Add the follow @using directives
//  We're now in a Libary and razor files don't have access to a _Imports.razor so we need to reference all the libraries we use
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using AllinOne.Shared.Data
// Add inheritance from ViewBase.  We get the the cascaded ViewManager and IView interface. 
@inherits ViewBase
// Set the namespace
@namespace AllinOne.Shared.Components
// Remove the @page directive.  It's redundant as we're not using routing
//@page "/"
```
*FetchData.razor*
```c#
//  Add the follow @using directives
//  We're now in a Libary and razor files don't have access to a _Imports.razor so we need to reference all the libraries we use
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using AllinOne.Shared.Data
// Add inheritance from ViewBase.  We get the the cascaded ViewManager and IView interface. 
@inherits ViewBase
@namespace AllinOne.Shared.Components
// Remove the @page directive.  It's redundant as we're not using routing
//@page "/fetchdata"
// Remove the @inject directive.  It's redundant as functionality has moved to the WeatherForecast data service
//@inject HttpClient Http

..... Markup

@code {

    // Injection of the IWeatherForecastService (WeatherForecastService in Server or WeatherForecastWASMService in WASM )
    [Inject] IWeatherForecastService DataService { get; set; }

    private WeatherForecast[] forecasts;

    // Get the data from the service
    protected override async Task OnInitializedAsync() => forecasts = await DataService.GetForecastAsync();
}
```

### Controls

Replace the default code in `ViewLink`.  This replicates `NavLink` providing application navigation between views.

```c#
using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace AllinOne.Shared.Components
{
    /// Builds a Bootstrap View Link to replicate NavLink but for View Navigation
    public class ViewLink : ComponentBase
    {
        /// View Type to Load
        [Parameter] public Type ViewType { get; set; }

        /// View Paremeters for the View
        [Parameter] public Dictionary<string, object> ViewParameters { get; set; } = new Dictionary<string, object>();

        /// Child Content to add to Component
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// Cascaded ViewManager
        [CascadingParameter] public ViewManager ViewManager { get; set; }

        /// Boolean to check if the ViewType is the current loaded view
        /// if so it's used to mark this component's CSS with "active" 
        private bool IsActive => this.ViewManager.IsCurrentView(this.ViewType);

        /// Captured Values
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> AdditionalAttributes { get; set; }

        /// Builds the render tree for the component
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var css = string.Empty;
            var viewData = new ViewData(ViewType, ViewParameters);

            if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out var obj))
            {
                css = Convert.ToString(obj, CultureInfo.InvariantCulture);
            }
            if (this.IsActive) css = $"{css} active";
            builder.OpenElement(0, "a");
            builder.AddAttribute(1, "class", css);
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            builder.AddAttribute(3, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, e => this.ViewManager.LoadViewAsync(viewData)));
            builder.AddContent(4, ChildContent);
            builder.CloseElement();
        }
    }
}
```

Update *NavMenu.razor* to handle switching between WASM and Server, and display which version we're running.

```html
//  Add the follow @using directives
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Routing
@using AllinOne.Shared.Components
//  And Namespace directive
@namespace AllinOne.Shared.Components

<div class="top-row pl-4 navbar navbar-dark">
    <a class="navbar-brand" href="">@navTitle</a>
    <button class="navbar-toggler" @onclick="ToggleNavMenu">
        <span class="navbar-toggler-icon"></span>
    </button>
</div>

<div class="@NavMenuCssClass" @onclick="ToggleNavMenu">
    <ul class="nav flex-column">
        <li class="nav-item px-3">
            @if (IsWASM)
            {
                <NavLink class="nav-link" href="/">
                    <span class="oi oi-browser" aria-hidden="true"></span> Switch to Server
                </NavLink>
            }
            else
            {
                <NavLink class="nav-link" href="wasm.html">
                    <span class="oi oi-browser" aria-hidden="true"></span> Switch to WASM
                </NavLink>

            }
        </li>
        <li class="nav-item px-3">
            <ViewLink class="nav-link" ViewType="typeof(Index)">
                <span class="oi oi-home" aria-hidden="true"></span> Home
            </ViewLink>
        </li>
        <li class="nav-item px-3">
            <ViewLink class="nav-link" ViewType="typeof(Counter)">
                <span class="oi oi-home" aria-hidden="true"></span> Counter
            </ViewLink>
        </li>
        <li class="nav-item px-3">
            <ViewLink class="nav-link" ViewType="typeof(FetchData)">
                <span class="oi oi-home" aria-hidden="true"></span> Fetch Data
            </ViewLink>
        </li>
    </ul>
</div>
```
```c#

@code {
    [Parameter] public bool IsWASM { get; set; }

    private string navTitle => IsWASM ? "All-in-One WASM" : "All-in-One Server";

    private bool collapseNavMenu = true;

    private string NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }
}
```

Update *SurveyPrompt.razor*

```c#
//  Add the follow @using directives
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Routing
//  And Namespace directive
@namespace AllinOne.Shared.Components
```

Update *MainLayout*

```c#
//  Add the follow @using directives
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Routing
@inherits LayoutComponentBase
//  And Namespace directive
@namespace AllinOne.Shared.Components
```

Copy the contents of `MainLayout` into `ServerLayout` and `WASMLayout`.

Update `WASMLayout` - `<NavMenu IsWASM="true" />`

```c#
// Section to update parameter ISWASM is set to true - we don't need to change ServerLayout as default is false
<div class="sidebar">
    <NavMenu IsWASM="true" />
</div>
```

`MainLayout` is now redundant. Delete it.

Update *Server.razor* replacing the default code with:

```c#
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Web

@namespace AllinOne.Shared.Components

// We define the ViewManager Component with the home View and default Layout
<ViewManager DefaultViewData="this.viewData" DefaultLayout="typeof(ServerLayout)">
</ViewManager>

@code {
    public ViewData viewData = new ViewData(typeof(AllinOne.Shared.Components.Index), null);
}
```

Update *WASM.razor* replacing the default code with:

```c#
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Web

@namespace AllinOne.Shared.Components

// We define the ViewManager Component with the home View and default Layout
// We use the WASMLayout which sets the NavMenu to the WASM version
<ViewManager DefaultViewData="this.viewData" DefaultLayout="typeof(WASMLayout)">
</ViewManager>

@code {
    public ViewData viewData = new ViewData(typeof(AllinOne.Shared.Components.Index), null);
}
```

### Server Project

#### Pages

Copy the following code into *_Host.cshtml* to *Pages* in the Server project.  This is the home page for the Server SPA.

```html
@page "/"
@using AllinOne.Shared.Components
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@{
    Layout = null;
}

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Blazor-Server</title>
    <base href="~/" />
    <link rel="stylesheet" href="css/bootstrap/bootstrap.min.css" />
    <link href="css/app.css" rel="stylesheet" />
</head>
<body>
    <app>
        <component type="typeof(AllinOne.Shared.Components.Server)" render-mode="ServerPrerendered" />
    </app>

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
#### Startup Files

Update *Program* in the Client project

```c#
// add reference 
using AllinOne.Shared.Data;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        // Sets the component to substitute into "app". 
        builder.RootComponents.Add < AllinOne.Shared.Components.WASM>("app");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        // add the WASM version of the weather dataservice
        builder.Services.AddScoped<IWeatherForecastService, WeatherForecastWASMService>();

        await builder.Build().RunAsync();
    }
}
```

Update *Startup* in the Server project

```c#
// add reference 
using AllinOne.Shared.Data;


public void ConfigureServices(IServiceCollection services)
{
    services.AddControllersWithViews();
    services.AddRazorPages();
    // Add Server Side Blazor services
    services.AddServerSideBlazor();
    // Add the server version of the weather dataservice
    services.AddSingleton<IWeatherForecastService, WeatherForecastService>();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        // Add the Blazor Hub
        endpoints.MapBlazorHub();
        endpoints.MapRazorPages();
        endpoints.MapControllers();
        // Set the default endpoint to _Host.cshtml
        endpoints.MapFallbackToPage("/_Host");
        // endpoints.MapFallbackToFile("index.html");
    });
}
```
## Final Clean Up

Clean up the project files.  Depending on how you moved the files around there may be some Folder/File artifacts left in the project files.  Most are harmless, but some can cause build problems (duplicate names).

An example of a harmless one:

```xml
  <ItemGroup>
    <Folder Include="Components\ViewManager\" />
  </ItemGroup>
```


 *AllinOne.Client.csproj*.

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <RazorLangVersion>3.0</RazorLangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="3.2.1" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Build" Version="3.2.1" PrivateAssets="all" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="3.2.1" PrivateAssets="all" />
    <PackageReference Include="System.Net.Http.Json" Version="3.2.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Shared\AllinOne.Shared.csproj" />
  </ItemGroup>

</Project>
```

 *AllinOne.Server.csproj*.

```xml
<<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Server" Version="3.2.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Client\AllinOne.Client.csproj" />
    <ProjectReference Include="..\Shared\AllinOne.Shared.csproj" />
  </ItemGroup>


</Project>
```

 *AllinOne.Shared.csproj*.

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <AssemblyName>AllinOne.Shared</AssemblyName>
    <RazorLangVersion>3.0</RazorLangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="3.2.1" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="3.2.1" PrivateAssets="all" />
    <PackageReference Include="Microsoft.AspNetCore.WebUtilities" Version="2.2.0" />
    <PackageReference Include="System.Net.Http.Json" Version="3.2.0" />
  </ItemGroup>

</Project>
```

## Build and run the Solution

That's it.  If you've done everything perfectly, and I've got everything right, you'll be able to build and run the project.  I've tested this set of instructions twice now, so I'm hoping I've not overlooked anything.  Comment and call me stupid if I have! 

Build errors will normally be because:

- missing assembly references
- dirty project files
- Comments marked with // in razor files.  They don't work in code but do in MD files (which is what this is written in).

## Wrap Up

Hopefully I've demonstrated that it's possible to develop, build and host both Blazor WASM and Server SPA's within the same solution on the same web site.  You do need to change your perspective on routing.  It took me a while to be convinced, but it's now my normal development mode.  An SPA is an application, not a web site.

To summarize what we have done:

1. Moved ALL the project code into the *Shared* library. Look at the completed contents of the *Client* and *Server* projects.
2. Created a data model that supports both Server and WASM data access.
3. Converted the *Server* project to run both the API and the Blazor Server side of the application.

The application starts in Server mode.  Clicking on the switch button loads *wasm.html* which loads the WASM application.  The seamless linking of the Client WASM objects into the Server site is handled by the *Microsoft.AspNetCore.Components.WebAssembly.Server* package.

The project is a prototype demonstrator and a bit rough around the edges.  It's not the polished ready to release article. 

Any problems with builds, post a comment at the end of this article - a link to a GitHub repo of your code would really help.

