using Microsoft.AspNetCore.Mvc;
namespace RaceDayMVC.Controllers;
public class HomeController : Controller {
 public IActionResult Index() => UserIsLoggedIn() ? RedirectToAction("Dashboard",HttpContext.Session.GetString("Role"))! : View();
 private bool UserIsLoggedIn()=>HttpContext.Session.GetInt32("UserID").HasValue;
}
