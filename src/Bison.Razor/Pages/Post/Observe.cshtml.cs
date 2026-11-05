using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages.Post;


// only allow logged in users to create new observations
[Authorize]
public class ObserveModel : PageModel
{
    private readonly IObservationService _service;

    public ObserveModel(IObservationService service)
    {
        _service = service;
    }

    
    [BindProperty]
    public string Message { get; set; } = "";

    public IActionResult OnPost()
    {
        var Author = User.Identity?.Name;
        if (string.IsNullOrEmpty(Author))
        {
            return RedirectToPage("/Identity/Account/Login", new { ReturnUrl = "/Observe" });
        }
        _service.AddObservation(Author, Message);
        var observationID = _service.GetObservationsFromAuthor(Author, 1).FirstOrDefault()?.ObservationId;
        return Redirect("/ob/" + observationID);
    }
}