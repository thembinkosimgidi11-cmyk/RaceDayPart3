using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers;

public class AuthController : Controller
{
    private readonly RaceDayAuthenticationService _authentication;

    public AuthController(RaceDayAuthenticationService authentication)
    {
        _authentication = authentication;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            var user = await _authentication.AuthenticateAsync(vm.Email, vm.Password);
            if (user is null)
            {
                ViewBag.Error = "Login failed. Check your email and password.";
                return View(vm);
            }

            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("DisplayName", $"{user.FirstName} {user.LastName}".Trim());

            return RedirectToAction("Dashboard", user.Role);
        }
        catch (Exception)
        {
            ViewBag.Error = "Unable to connect to RaceDayDB. Check the SQL Server connection string and make sure the database is online.";
            return View(vm);
        }
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM vm)
    {
        vm.Role = (vm.Role ?? "").Trim();

        if (string.IsNullOrWhiteSpace(vm.FirstName))
            ModelState.AddModelError(nameof(vm.FirstName), "First name is required.");
        if (string.IsNullOrWhiteSpace(vm.LastName))
            ModelState.AddModelError(nameof(vm.LastName), "Last name is required.");
        if (!IsValidEmail(vm.Email))
            ModelState.AddModelError(nameof(vm.Email), "Enter a valid email address.");
        if (vm.Password is null || vm.Password.Length < 8)
            ModelState.AddModelError(nameof(vm.Password), "Use at least 8 characters.");

        if (vm.Role is not ("Organiser" or "Participant"))
            ModelState.AddModelError(nameof(vm.Role), "Choose Organiser or Participant.");

        if (vm.Role == "Organiser")
        {
            if (string.IsNullOrWhiteSpace(vm.Phone))
                ModelState.AddModelError(nameof(vm.Phone), "Phone number is required.");
            if (string.IsNullOrWhiteSpace(vm.Organisation))
                ModelState.AddModelError(nameof(vm.Organisation), "Organisation is required.");
        }
        else if (vm.Role == "Participant")
        {
            if (string.IsNullOrWhiteSpace(vm.Phone))
                ModelState.AddModelError(nameof(vm.Phone), "Phone number is required.");
            if (vm.DateOfBirth is null || vm.DateOfBirth.Value.Date > DateTime.Today)
                ModelState.AddModelError(nameof(vm.DateOfBirth), "Enter a valid date of birth.");
            if (vm.Gender is not ("Male" or "Female" or "Other"))
                ModelState.AddModelError(nameof(vm.Gender), "Choose a valid gender.");
            if (string.IsNullOrWhiteSpace(vm.EmergencyContact))
                ModelState.AddModelError(nameof(vm.EmergencyContact), "Emergency contact is required.");
            if (string.IsNullOrWhiteSpace(vm.EmergencyPhone))
                ModelState.AddModelError(nameof(vm.EmergencyPhone), "Emergency phone is required.");
        }

        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            await _authentication.RegisterAsync(vm);
            TempData["Message"] = "Registration successful. You can now log in.";
            return RedirectToAction(nameof(Login));
        }
        catch (DuplicateAccountException)
        {
            ViewBag.Error = "An account with this email address already exists.";
            return View(vm);
        }
        catch (Exception)
        {
            ViewBag.Error = "Registration could not be completed. Check that RaceDayDB is installed and that the SQL Server connection string is correct.";
            return View(vm);
        }
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        try { return new MailAddress(email).Address == email.Trim(); }
        catch (FormatException) { return false; }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
