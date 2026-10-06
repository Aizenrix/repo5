using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TodoManager.Models;
namespace TodoManager.Authorization;
public class CanEditTaskHandler : AuthorizationHandler<CanEditTaskRequirement, TaskItem>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanEditTaskRequirement requirement,
        TaskItem resource)
    {
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Task.CompletedTask;
        if (resource.UserId == userId || context.User.IsInRole("Admin"))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
