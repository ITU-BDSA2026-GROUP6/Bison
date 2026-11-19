using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Bison.Razor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bison.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly IObservationService _service;
    public List<ObservationViewModel> Observations { get; set; } = [];
    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
    public List<SelectListItem> Taxons { get; set; } = new();

    [BindProperty]
    public int? SelectedTaxonId { get; set; }

    public PublicModel(IObservationService service)
    {
        _service = service;
    }
    public IActionResult OnGet([FromQuery] int page = 1)
    {
        LoadTaxons();

        CurrentPage = Math.Max(page, 1);
        TotalPages = Math.Max(1, (int)Math.Ceiling(
            _service.GetObservationCount() / (double)ObservationService.PageSize));
        CurrentPage = Math.Min(CurrentPage, TotalPages);
        Observations = _service.GetObservations(CurrentPage);
        
        return Page();
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
