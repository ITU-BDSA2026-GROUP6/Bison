using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bison.Razor.Pages.Post;



// only allow logged in users to create new observations
[Authorize]
public class ObserveModel : PageModel
{
    private readonly IObservationService _service;
    private readonly BisonDbContext _context;

    public ObserveModel(IObservationService service, BisonDbContext context)
    {
        _service = service;
        _context = context;
    }







    [BindProperty]
    public string Message { get; set; } = "";

    public List<SelectListItem> Taxons { get; set; } = new();

    [BindProperty]
    public int? SelectedTaxonId { get; set; }

    public IActionResult OnGet()
    {
        LoadTaxons();
        return Page();
    }
    public IActionResult OnPost()
    {
        var Author = User.Identity?.Name;
        if (string.IsNullOrEmpty(Author))
        {
            return RedirectToPage("/Identity/Account/Login", new { ReturnUrl = "/Observe" });
        }

        if (!ModelState.IsValid)
        {
            LoadTaxons();
            return Page();
        }

        if (SelectedTaxonId.HasValue && !_context.Taxons.Any(t => t.TaxonId == SelectedTaxonId.Value))
        {
            ModelState.AddModelError("SelectedTaxonId", "Please select a valid taxon.");
            LoadTaxons();
            return Page();
        }

        _service.AddObservation(Author, Message, SelectedTaxonId);

        var observationID = _service.GetObservationsFromAuthor(Author, 1).FirstOrDefault()?.ObservationId;

        return Redirect("/ob/" + observationID);
    }
    private void LoadTaxons()
    {
        Taxons = _context.Taxons.OrderBy(t => t.VernacularName).Select(t => new SelectListItem
        {
            Value = t.TaxonId.ToString(),
            Text = t.VernacularName
        }).ToList();
    }
}