using Bison.Razor.Models;
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

    public ObserveModel(IObservationService service)
    {
        _service = service;
    }

    private const long MaxPictureBytes = 2 * 1024 * 1024;

    [BindProperty]
    public IFormFile? Upload { get; set; }

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

        var picture = ReadPicture();
        if (!ModelState.IsValid)
        {
            LoadTaxons();
            return Page();
        }

        if (SelectedTaxonId.HasValue && !_service.GetTaxons().Any(t => t.TaxonId == SelectedTaxonId.Value))
        {
            ModelState.AddModelError("SelectedTaxonId", "Please select a valid taxon.");
            LoadTaxons();
            return Page();
        }

        _service.AddObservation(Author, Message, SelectedTaxonId, picture);

        var observationID = _service.GetObservationsFromAuthor(Author, 1).FirstOrDefault()?.ObservationId;

        return Redirect("/ob/" + observationID);
    }
    private Picture? ReadPicture()
    {
        if (Upload is null || Upload.Length == 0) return null;
        if (Upload.Length > MaxPictureBytes)
        {
            ModelState.AddModelError(nameof(Upload), "The picture must be 2 MB or smaller.");
            return null;
        }

        using var stream = new MemoryStream();
        Upload.CopyTo(stream);
        var data = stream.ToArray();

        var contentType = Picture.DetectContentType(data);
        if (contentType is null)
        {
            ModelState.AddModelError(nameof(Upload), "The picture must be a JPEG or PNG.");
            return null;
        }
        return new Picture { Data = data, ContentType = contentType };
    }

    private void LoadTaxons()
    {
        Taxons = _service.GetTaxons()
            .OrderBy(t => t.VernacularName)
            .Select(t => new SelectListItem
            {   
                Value = t.TaxonId.ToString(),   
                Text = t.VernacularName
            }).ToList();
    }
}