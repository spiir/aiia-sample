using System.Threading.Tasks;
using Aiia.Sample.Models;
using Microsoft.AspNetCore.Mvc;

namespace Aiia.Sample.ViewComponents;

public class ObjectDetailsViewComponent : ViewComponent
{
    public ObjectDetailsViewComponent()
    {
        // add dependency injection here if needed
    }

    public async Task<IViewComponentResult> InvokeAsync(object objectToShow)
    {
        return View(new ObjectDetailsViewModel(objectToShow));
    }
}
