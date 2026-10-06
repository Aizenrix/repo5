using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoManager.Data;
using TodoManager.Models;
namespace TodoManager.Pages.Tasks;
[Authorize]
public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthorizationService _authService;
    public EditModel(AppDbContext db, IAuthorizationService auth) { _db = db; _authService = auth; }
    [BindProperty]
    public InputModel Input { get; set; } = new();
    public class InputModel
    {
        public int Id { get; set; }
        [Required] public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
    }
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        var auth = await _authService.AuthorizeAsync(User, task, "CanEditTask");
        if (!auth.Succeeded) return Forbid();
        Input = new InputModel { Id = task.Id, Title = task.Title, Description = task.Description, IsCompleted = task.IsCompleted };
        return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var task = await _db.Tasks.FindAsync(Input.Id);
        if (task == null) return NotFound();
        var auth = await _authService.AuthorizeAsync(User, task, "CanEditTask");
        if (!auth.Succeeded) return Forbid();
        task.Title = Input.Title;
        task.Description = Input.Description;
        task.IsCompleted = Input.IsCompleted;
        await _db.SaveChangesAsync();
        return RedirectToPage("/Tasks/Index");
    }
}
