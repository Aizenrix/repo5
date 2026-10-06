using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoManager.Models;
namespace TodoManager.Pages.Admin;
[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    public IndexModel(UserManager<AppUser> um) => _userManager = um;
    public List<UserViewModel> Users { get; set; } = new();
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string FullName { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }
    public async Task OnGetAsync()
    {
        foreach (var user in _userManager.Users.ToList())
        {
            Users.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = await _userManager.GetRolesAsync(user)
            });
        }
    }
    public async Task<IActionResult> OnPostChangeRoleAsync(string userId, string newRole)
    {
        var currentUserId = _userManager.GetUserId(User);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        if (userId == currentUserId && newRole != "Admin")
        {
            ModelState.AddModelError("", "Нельзя снять роль Admin с самого себя.");
            await OnGetAsync();
            return Page();
        }
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, newRole);
        return RedirectToPage();
    }
}
