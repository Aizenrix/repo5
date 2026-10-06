using Microsoft.AspNetCore.Identity;
namespace TodoManager.Models;
public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
