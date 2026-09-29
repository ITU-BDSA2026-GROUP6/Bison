using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly IObservationService _service;
    public List<ObservationViewModel> Observations { get; set; }

    public PublicModel(IObservationService service)
    {
        _service = service;
    }
    public IActionResult OnGet([FromQuery] int page = 1)
    {
        if (page < 1) page = 1;
        Observations = _service.GetObservations(page);
        return Page();
    }
}
