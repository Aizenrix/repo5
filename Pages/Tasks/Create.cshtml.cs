using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoManager.Data;
using TodoManager.Models;
namespace TodoManager.Pages.Tasks;
[Authorize]
public class CreateModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    public CreateModel(AppDbContext db, UserManager<AppUser> um) { _db = db; _userManager = um; }
    [BindProperty]
    public InputModel Input { get; set; } = new();
    public class InputModel
    {
        [Required] public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var task = new TaskItem
        {
            Title = Input.Title,
            Description = Input.Description,
            UserId = _userManager.GetUserId(User)!,
            CreatedAt = DateTime.UtcNow
        };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return RedirectToPage("/Tasks/Index");
    }
}
