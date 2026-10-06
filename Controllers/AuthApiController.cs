using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoManager.Data;
using TodoManager.Models;
namespace TodoManager.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public AuthApiController(UserManager<AppUser> um, SignInManager<AppUser> sm, IConfiguration config, AppDbContext db)
    {
        _userManager = um; _signInManager = sm; _config = config; _db = db;
    }

    public record LoginRequest(string Email, string Password);
    public record RefreshRequest(string RefreshToken);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user == null) return Unauthorized("Неверные данные.");
        var result = await _signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
        if (!result.Succeeded) return Unauthorized("Неверный пароль или аккаунт заблокирован.");
        var accessToken = await GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken(user.Id);
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();
        return Ok(new { accessToken, refreshToken = refreshToken.Token });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var token = await _db.RefreshTokens.Include(r => r.User).FirstOrDefaultAsync(r => r.Token == req.RefreshToken);
        if (token == null || !token.IsActive) return Unauthorized("Недействительный refresh token.");
        token.RevokedAt = DateTime.UtcNow;
        var newRefresh = GenerateRefreshToken(token.UserId);
        _db.RefreshTokens.Add(newRefresh);
        await _db.SaveChangesAsync();
        var accessToken = await GenerateJwtToken(token.User!);
        return Ok(new { accessToken, refreshToken = newRefresh.Token });
    }

    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == req.RefreshToken);
        if (token != null && token.IsActive)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        return Ok("Выход выполнен.");
    }

    [HttpPost("logout-all")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tokens = await _db.RefreshTokens.Where(r => r.UserId == userId && r.RevokedAt == null).ToListAsync();
        foreach (var t in tokens) t.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok("Все сессии завершены.");
    }

    private async Task<string> GenerateJwtToken(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = int.TryParse(_config["Jwt:ExpiryMinutes"], out var min) ? min : 60;
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static RefreshToken GenerateRefreshToken(string userId) => new()
    {
        Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
        UserId = userId,
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        CreatedAt = DateTime.UtcNow
    };
}
