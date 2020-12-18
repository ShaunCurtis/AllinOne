using System;
using System.Threading.Tasks;

namespace AllinOne.Shared.Components.Controls
{
    class EventCounter : BaseCounter
    {
        protected override string buttoncolor => "btn-success";

        protected override Task OnInitializedAsync()
        {
            Service.CounterChanged += ReRender;
            return base.OnInitializedAsync();
        }

        protected void ReRender(object sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);
    }

}
