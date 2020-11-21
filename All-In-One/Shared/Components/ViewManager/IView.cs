using Microsoft.AspNetCore.Components;
using System;

namespace AllinOne.Shared.Components
{
    public interface IView : IComponent
    {
        [CascadingParameter] public ViewManager ViewManager { get; set; }
    }
}
