using Microsoft.AspNetCore.Mvc;
namespace FoodManager_BIT240194.Controllers;
public class HomeController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        Response.StatusCode = 500;
        ViewData["Code"] = 500;
        return View("Status");
    }
    [ActionName("Status")]
    public IActionResult StatusPage(int code)
    {
        Response.StatusCode = code;
        ViewData["Code"] = code;
        return View("Status");
    }
}
