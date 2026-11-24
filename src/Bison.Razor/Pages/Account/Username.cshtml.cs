using System.ComponentModel.DataAnnotations;
using Bison.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Pages.Account;

[Authorize]
public class UsernameModel : PageModel
{
    private readonly UserManager<BisonUser> _userManager;
    private readonly SignInManager<BisonUser> _signInManager;
    private readonly BisonDbContext _context;

    public UsernameModel(
        UserManager<BisonUser> userManager,
        SignInManager<BisonUser> signInManager,
        BisonDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [BindProperty]
    [Required]
    [StringLength(30, MinimumLength = 3)]
    [RegularExpression(
        @"^[a-zA-Z0-9_-]+$",
        ErrorMessage =
            "Use only letters, numbers, underscores, and hyphens.")]
    public string Username { get; set; } = "";

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        Username = user.UserName ?? "";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var author = await _context.Authors
            .SingleOrDefaultAsync(a => a.BisonUserId == user.Id);

        // Avoid creating a second author for existing unlinked posts.
        if (author == null && await _context.Authors
            .AnyAsync(a => a.Name == user.UserName))
        {
            ModelState.AddModelError(
                "", "Your existing author must be linked first.");
            return Page();
        }

        if (await _context.Authors.AnyAsync(a =>
            a.Name.ToUpper() == Username.ToUpper() &&
            a.BisonUserId != user.Id))
        {
            ModelState.AddModelError(
                nameof(Username), "That author name is taken.");
            return Page();
        }

        using var transaction =
            await _context.Database.BeginTransactionAsync();

        var result = await _userManager.SetUserNameAsync(user, Username);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    nameof(Username), error.Description);
            }

            return Page();
        }

        if (author == null)
        {
            author = new Author { BisonUserId = user.Id };
            _context.Authors.Add(author);
        }

        author.Name = Username;
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your username has been updated.";
        return RedirectToPage();
    }
}