using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoManager.Data;
using TodoManager.Models;
namespace TodoManager.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TasksApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public TasksApiController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        var tasks = isAdmin
            ? await _db.Tasks.ToListAsync()
            : await _db.Tasks.Where(t => t.UserId == userId).ToListAsync();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        if (task.UserId != userId && !User.IsInRole("Admin")) return Forbid();
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var task = new TaskItem { Title = req.Title, Description = req.Description, UserId = userId, CreatedAt = DateTime.UtcNow };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateTaskRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        if (task.UserId != userId && !User.IsInRole("Admin")) return Forbid();
        task.Title = req.Title;
        task.Description = req.Description;
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        if (task.UserId != userId && !User.IsInRole("Admin")) return Forbid();
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        if (task.UserId != userId && !User.IsInRole("Admin")) return Forbid();
        task.IsCompleted = true;
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    public record CreateTaskRequest(string Title, string? Description);
}
