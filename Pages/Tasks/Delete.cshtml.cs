using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoManager.Data;
using TodoManager.Models;
namespace TodoManager.Pages.Tasks;
[Authorize]
public class DeleteModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthorizationService _authService;
    public DeleteModel(AppDbContext db, IAuthorizationService auth) { _db = db; _authService = auth; }
    public TaskItem? Task { get; set; }
    public async Task<IActionResult> OnGetAsync(int id)
    {
        Task = await _db.Tasks.FindAsync(id);
        if (Task == null) return NotFound();
        var auth = await _authService.AuthorizeAsync(User, Task, "CanEditTask");
        if (!auth.Succeeded) return Forbid();
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        var auth = await _authService.AuthorizeAsync(User, task, "CanEditTask");
        if (!auth.Succeeded) return Forbid();
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return RedirectToPage("/Tasks/Index");
    }
}
