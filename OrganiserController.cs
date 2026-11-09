using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
namespace RaceDayMVC.Controllers;
public class OrganiserController : Controller {
 private readonly RaceDayApiClient _api; public OrganiserController(RaceDayApiClient api)=>_api=api;
 private bool Allowed()=>HttpContext.Session.GetString("Role")=="Organiser";
 public async Task<IActionResult> Dashboard(){if(!Allowed())return RedirectToAction("Login","Auth"); var uid=HttpContext.Session.GetInt32("UserID"); var ev=await _api.Get<List<ApiEvent>>("api/Events")??new(); ViewBag.EventCount=ev.Count(e=>e.OrganiserID==uid); ViewBag.Enrolments=0; return View(ev.Where(e=>e.OrganiserID==uid).ToList());}
 public async Task<IActionResult> Events(){if(!Allowed())return RedirectToAction("Login","Auth");var uid=HttpContext.Session.GetInt32("UserID");var all=await _api.Get<List<ApiEvent>>("api/Events")??new();return View(all.Where(e=>e.OrganiserID==uid).ToList());}
 [HttpGet] public IActionResult CreateEvent(){if(!Allowed())return RedirectToAction("Login","Auth");return View(new EventVM());}
 [HttpPost] public async Task<IActionResult> CreateEvent(EventVM vm){if(!Allowed())return RedirectToAction("Login","Auth");if(!ModelState.IsValid)return View(vm);var uid=HttpContext.Session.GetInt32("UserID")??0;var r=await _api.Post("api/Events",new {organiserID=uid,eventName=vm.EventName,description=vm.Description,eventDate=vm.EventDate,startTime=vm.StartTime,location=vm.Location,status=vm.Status});if(!r.Ok){ViewBag.Error="API error: "+r.Message;return View(vm);}return RedirectToAction(nameof(Events));}
 public async Task<IActionResult> Categories(int eventId){if(!Allowed())return RedirectToAction("Login","Auth");ViewBag.EventID=eventId;var all=await _api.Get<List<ApiCategory>>("api/Categories")??new();return View(all.Where(c=>c.EventID==eventId).ToList());}
 [HttpGet] public IActionResult CreateCategory(int eventId){if(!Allowed())return RedirectToAction("Login","Auth");return View(new CategoryVM{EventID=eventId});}
 [HttpPost] public async Task<IActionResult> CreateCategory(CategoryVM vm){if(!Allowed())return RedirectToAction("Login","Auth");var r=await _api.Post("api/Categories",new {eventID=vm.EventID,categoryName=vm.CategoryName,distanceKm=vm.DistanceKm,entryFee=vm.EntryFee,maxParticipants=vm.MaxParticipants});if(!r.Ok){ViewBag.Error="API error: "+r.Message;return View(vm);}return RedirectToAction(nameof(Categories),new{eventId=vm.EventID});}
 public IActionResult Enrolments(){if(!Allowed())return RedirectToAction("Login","Auth");return View();}
 public IActionResult Results(){if(!Allowed())return RedirectToAction("Login","Auth");ViewBag.Note="The Results API controller in the supplied ZIP is empty. Implement the Results endpoints before this page can display or publish results.";return View();}
}
