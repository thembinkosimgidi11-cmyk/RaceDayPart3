using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
namespace RaceDayMVC.Controllers;
public class ParticipantController : Controller {
 private readonly RaceDayApiClient _api; public ParticipantController(RaceDayApiClient api)=>_api=api;
 private bool Allowed()=>HttpContext.Session.GetString("Role")=="Participant";
 public async Task<IActionResult> Dashboard(){if(!Allowed())return RedirectToAction("Login","Auth");var events=await _api.Get<List<ApiEvent>>("api/Events")??new();return View(events);}
 public async Task<IActionResult> Events(){if(!Allowed())return RedirectToAction("Login","Auth");var events=await _api.Get<List<ApiEvent>>("api/Events")??new();return View(events);}
 [HttpGet] public async Task<IActionResult> Enrol(int categoryId){if(!Allowed())return RedirectToAction("Login","Auth");var cats=await _api.Get<List<ApiCategory>>("api/Categories")??new();var c=cats.FirstOrDefault(x=>x.CategoryID==categoryId);if(c==null)return NotFound();return View(new EnrolVM{CategoryID=categoryId});}
 [HttpPost] public async Task<IActionResult> Enrol(EnrolVM vm){if(!Allowed())return RedirectToAction("Login","Auth");var users=await _api.Get<List<ApiUser>>("api/Users")??new();var uid=HttpContext.Session.GetInt32("UserID");var participant=users.FirstOrDefault(u=>u.UserID==uid); // Participant profile is obtained below from the Participants endpoint.
 var participants=await _api.Get<List<ParticipantApi>>("api/Participants")??new();var p=participants.FirstOrDefault(x=>x.UserID==uid);
 if(p==null){ViewBag.Error="No Participant profile exists for this account. The API must create a Participant record during registration.";return View(vm);}
 var result=await _api.Post("api/Enrollments",new{participantID=p.ParticipantID,categoryID=vm.CategoryID,raceNumber=$"RD-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",status="Registered"});
 if(!result.Ok){ViewBag.Error="Enrolment failed: "+result.Message;return View(vm);}TempData["Message"]="You have successfully enrolled.";return RedirectToAction(nameof(MyEvents));}
 public async Task<IActionResult> MyEvents(){if(!Allowed())return RedirectToAction("Login","Auth");var participants=await _api.Get<List<ParticipantApi>>("api/Participants")??new();var p=participants.FirstOrDefault(x=>x.UserID==HttpContext.Session.GetInt32("UserID"));if(p==null)return View(new List<ApiEnrollment>());var enrollments=await _api.Get<List<ApiEnrollment>>("api/Enrollments")??new();return View(enrollments.Where(e=>e.ParticipantID==p.ParticipantID).ToList());}
 public async Task<IActionResult> Results(){if(!Allowed())return RedirectToAction("Login","Auth");var participants=await _api.Get<List<ParticipantApi>>("api/Participants")??new();var p=participants.FirstOrDefault(x=>x.UserID==HttpContext.Session.GetInt32("UserID"));var enrollments=await _api.Get<List<ApiEnrollment>>("api/Enrollments")??new();return View(p==null?new():enrollments.Where(e=>e.ParticipantID==p.ParticipantID&&e.Result!=null).ToList());}
 private sealed class ParticipantApi{public int ParticipantID{get;set;}public int UserID{get;set;}}
}
