using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
namespace FoodManager_BIT240194.Controllers;
public class LanguageController : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Set(string culture, string? returnUrl)
    {
        if (culture != "en" && culture != "vi") return BadRequest();
        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax, Secure = Request.IsHttps });
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }
}
