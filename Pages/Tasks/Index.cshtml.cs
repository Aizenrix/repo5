using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TodoManager.Data;
using TodoManager.Models;
namespace TodoManager.Pages.Tasks;
[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly IAuthorizationService _authService;
    public List<TaskItem> Tasks { get; set; } = new();
    public IndexModel(AppDbContext db, UserManager<AppUser> um, IAuthorizationService auth)
    {
        _db = db; _userManager = um; _authService = auth;
    }
    public async Task OnGetAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (User.IsInRole("Admin"))
            Tasks = await _db.Tasks.ToListAsync();
        else
            Tasks = await _db.Tasks.Where(t => t.UserId == userId).ToListAsync();
    }
    public async Task<IActionResult> OnPostCompleteAsync(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        var auth = await _authService.AuthorizeAsync(User, task, "CanEditTask");
        if (!auth.Succeeded) return Forbid();
        task.IsCompleted = true;
        await _db.SaveChangesAsync();
        return new Microsoft.AspNetCore.Mvc.JsonResult(new { success = true });
    }
}
