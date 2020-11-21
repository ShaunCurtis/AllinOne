using Microsoft.AspNetCore.Components;

namespace AllinOne.Shared.Components
{
    public class ViewBase : ComponentBase, IView
{
        [CascadingParameter]
        public ViewManager ViewManager { get; set; }
    }
}
