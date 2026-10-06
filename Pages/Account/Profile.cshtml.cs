using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;
using TodoManager.Models;
namespace TodoManager.Pages.Account;
[Authorize]
public class ProfileModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly UrlEncoder _urlEncoder;
    public ProfileModel(UserManager<AppUser> um, UrlEncoder ue) { _userManager = um; _urlEncoder = ue; }
    public string Email { get; set; } = string.Empty;
    public bool TwoFactorEnabled { get; set; }
    public string? QrCodeUri { get; set; }
    public string? SharedKey { get; set; }
    [BindProperty, StringLength(7, MinimumLength = 6)]
    public string VerificationCode { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;
        Email = user.Email ?? "";
        TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
    }

    public async Task<IActionResult> OnPostSetup2FAAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage();
        Email = user.Email ?? "";
        TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        await _userManager.ResetAuthenticatorKeyAsync(user);
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        SharedKey = FormatKey(key!);
        QrCodeUri = GenerateQrCodeUri(user.Email!, key!);
        return Page();
    }

    public async Task<IActionResult> OnPostEnable2FAAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage();
        Email = user.Email ?? "";
        TwoFactorEnabled = false;
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (key == null) { ModelState.AddModelError("", "Сначала настройте 2FA."); return Page(); }
        SharedKey = FormatKey(key);
        QrCodeUri = GenerateQrCodeUri(user.Email!, key);
        var token = VerificationCode.Replace(" ", "");
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, token);
        if (!isValid) { ModelState.AddModelError(nameof(VerificationCode), "Неверный код."); return Page(); }
        await _userManager.SetTwoFactorEnabledAsync(user, true);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDisable2FAAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null) await _userManager.SetTwoFactorEnabledAsync(user, false);
        return RedirectToPage();
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
            result.Append(unformattedKey.AsSpan(currentPosition));
        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string key)
    {
        var authenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        var otpUri = string.Format(authenticatorUriFormat, _urlEncoder.Encode("TodoManager"), _urlEncoder.Encode(email), key);
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(otpUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var bytes = qrCode.GetGraphic(10);
        return "data:image/png;base64," + Convert.ToBase64String(bytes);
    }
}
