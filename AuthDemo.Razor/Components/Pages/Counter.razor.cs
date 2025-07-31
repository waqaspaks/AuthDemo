using Microsoft.AspNetCore.Components;

namespace AuthDemo.Razor.Components.Pages
{
    public partial class Counter : ComponentBase
    {
        protected int currentCount = 0;

        protected void IncrementCount()
        {
            currentCount++;
        }
    }
}