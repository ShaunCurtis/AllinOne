// <auto-generated/>
#pragma warning disable 1591
#pragma warning disable 0414
#pragma warning disable 0649
#pragma warning disable 0169

namespace Blazor.Shared
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
#nullable restore
#line 1 "C:\Users\Shaun.Obsidian\source\repos\Blazor\Blazor\_Imports.razor"
using System.Net.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\Shaun.Obsidian\source\repos\Blazor\Blazor\_Imports.razor"
using System.Net.Http.Json;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "C:\Users\Shaun.Obsidian\source\repos\Blazor\Blazor\_Imports.razor"
using Microsoft.AspNetCore.Components.Forms;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "C:\Users\Shaun.Obsidian\source\repos\Blazor\Blazor\_Imports.razor"
using Microsoft.AspNetCore.Components.Routing;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "C:\Users\Shaun.Obsidian\source\repos\Blazor\Blazor\_Imports.razor"
using Microsoft.AspNetCore.Components.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 6 "C:\Users\Shaun.Obsidian\source\repos\Blazor\Blazor\_Imports.razor"
using Microsoft.AspNetCore.Components.Web.Virtualization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 7 "C:\Users\Shaun.Obsidian\source\repos\Blazor\Blazor\_Imports.razor"
using Microsoft.AspNetCore.Components.WebAssembly.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 8 "C:\Users\Shaun.Obsidian\source\repos\Blazor\Blazor\_Imports.razor"
using Microsoft.JSInterop;

#line default
#line hidden
#nullable disable
#nullable restore
#line 9 "C:\Users\Shaun.Obsidian\source\repos\Blazor\Blazor\_Imports.razor"
using Blazor;

#line default
#line hidden
#nullable disable
#nullable restore
#line 10 "C:\Users\Shaun.Obsidian\source\repos\Blazor\Blazor\_Imports.razor"
using Blazor.Data;

#line default
#line hidden
#nullable disable
#nullable restore
#line 11 "C:\Users\Shaun.Obsidian\source\repos\Blazor\Blazor\_Imports.razor"
using Blazor.Services;

#line default
#line hidden
#nullable disable
#nullable restore
#line 12 "C:\Users\Shaun.Obsidian\source\repos\Blazor\Blazor\_Imports.razor"
using Blazor.Shared;

#line default
#line hidden
#nullable disable
    public partial class MainLayout : LayoutComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
        }
        #pragma warning restore 1998
#nullable restore
#line 20 "C:\Users\Shaun.Obsidian\source\repos\Blazor\Blazor\Shared\MainLayout.razor"
       
    [Inject] NavigationManager NavManager { get; set; }
    private bool _isWasm => NavManager?.Uri.Contains("wasm", StringComparison.CurrentCultureIgnoreCase) ?? false;
    private string _sidebarCss => _isWasm ? "sidebar sidebar-teal" : "sidebar sidebar-steel";

#line default
#line hidden
#nullable disable
    }
}
#pragma warning restore 1591