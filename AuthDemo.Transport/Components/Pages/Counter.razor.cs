using Microsoft.AspNetCore.Components;

namespace AuthDemo.Transport.Components.Pages
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