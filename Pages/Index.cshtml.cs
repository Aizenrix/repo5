using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoManager.Models;
namespace TodoManager.Pages;
public class IndexModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    public string FullName { get; set; } = string.Empty;
    public IndexModel(UserManager<AppUser> userManager) => _userManager = userManager;
    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            FullName = user?.FullName ?? User.Identity.Name ?? "";
        }
    }
}
