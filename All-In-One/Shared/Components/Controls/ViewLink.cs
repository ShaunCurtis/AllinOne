using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace AllinOne.Shared.Components
{
    /// <summary>
    /// Builds a Bootstrap View Link
    /// </summary>
    public class ViewLink : ComponentBase
    {
        /// <summary>
        /// View Type to Load
        /// </summary>
        [Parameter] public Type ViewType { get; set; }

        /// <summary>
        /// View Paremeters for the View
        /// </summary>
        [Parameter] public Dictionary<string, object> ViewParameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Child Content to add to Component
        /// </summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Cascaded ViewManager
        /// </summary>
        [CascadingParameter] public ViewManager ViewManager { get; set; }

        /// <summary>
        /// Boolean to check if the ViewType is the current loaded view
        /// if so it's used to mark this component's CSS with "active" 
        /// </summary>
        private bool IsActive => this.ViewManager.IsCurrentView(this.ViewType);
        
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> AdditionalAttributes { get; set; }

        /// <summary>
        /// inherited
        /// Builds the render tree for the component
        /// </summary>
        /// <param name="builder"></param>
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
