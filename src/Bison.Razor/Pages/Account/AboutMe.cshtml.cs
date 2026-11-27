using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace Bison.Razor.Pages.Account;

[Authorize]
public class AboutMeModel : PageModel
{
    private readonly IObservationService _service;

    public AboutMeModel(IObservationService service)
    {
        _service = service;
    }

    public PersonalData Data {get;private set;} = default!;

    public IActionResult OnGet()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var data = _service.GetPersonalData(userId);
        if(data is null)
        {
            return NotFound();
        }
        Data = data;
        return Page();
    }
}
