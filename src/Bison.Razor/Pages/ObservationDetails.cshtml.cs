using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bison.Razor.Pages;

public class ObservationDetailsModel : PageModel
{
    private readonly IObservationService _service;

    public ObservationViewModel? Observation { get; private set; }
    public List<ObservationViewModel> Observations { get; private set; } = [];
    public List<CommentViewModel> Comments { get; private set; } = [];
    public List<ProposalViewModel> Proposals { get; private set; } = [];

    public List<SelectListItem> Taxons { get; set; } = new();

    [BindProperty]
    public string CommentText { get; set; } = "";

    [BindProperty]
    public string ProposalText { get; set; } = "";

    [BindProperty]
    public int? SelectedTaxonId { get; set; }

    public ObservationDetailsModel(IObservationService service)
    {
        _service = service;
    }

    public IActionResult OnGet(int? id, [FromQuery] int page = 1)
    {
        if (id is null)
        {
            Observations = _service.GetObservations(Math.Max(page, 1));
            return Page();
        }

        Observation = _service.GetObservationById(id.Value);

        if (Observation is null)
            return NotFound();

        Comments = _service.GetCommentsForObservation(id.Value);
        Proposals = _service.GetProposalsForObservation(id.Value);
        LoadTaxons();
        return Page();
    }

    public IActionResult OnPostComment(int id)
    {
        var author = User.Identity?.Name;
        if (string.IsNullOrEmpty(author))
        {
            return RedirectToPage("/Identity/Account/Login", new { ReturnUrl = "/ob/" + id });
        }

        if (string.IsNullOrWhiteSpace(CommentText))
        {
            return RedirectToPage(new { id });
        }

        _service.AddComment(author, id, CommentText);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostProposal(int id)
    {
        var author = User.Identity?.Name;
        if (string.IsNullOrEmpty(author))
        {
            return RedirectToPage("/Identity/Account/Login", new { ReturnUrl = "/ob/" + id });
        }

        if (string.IsNullOrWhiteSpace(ProposalText) || !SelectedTaxonId.HasValue)
        {
            return RedirectToPage(new { id });
        }

        _service.AddProposal(author, id, SelectedTaxonId.Value, ProposalText);
        return RedirectToPage(new { id });
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