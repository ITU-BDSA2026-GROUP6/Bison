using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages;

public class ObservationDetailsModel : PageModel
{
    private readonly IObservationService _service;

    public ObservationViewModel? Observation { get; private set; }
    public List<ObservationViewModel> Observations { get; private set; } = [];
    public List<CommentViewModel> Comments { get; private set; } = [];
    public List<ProposalViewModel> Proposals { get; private set; } = [];

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
        return Page();
    }
}