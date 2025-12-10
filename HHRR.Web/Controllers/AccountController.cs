using HHRR.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HHRR.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login()
    {
        Console.WriteLine("[DEBUG] AccountController: GET Login");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        Console.WriteLine($"[DEBUG] AccountController: POST Login for {model.Email}");

        if (ModelState.IsValid)
        {
            // Check if user exists first for debugging
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                Console.WriteLine($"[DEBUG] User {model.Email} NOT FOUND in DB.");
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                Console.WriteLine("[DEBUG] Login SUCCESS. Redirecting to Home/Index.");
                return RedirectToAction("Index", "Home");
            }
            
            if (result.IsLockedOut) Console.WriteLine("[DEBUG] Login FAILED: Locked Out.");
            if (result.IsNotAllowed) Console.WriteLine("[DEBUG] Login FAILED: Not Allowed.");
            if (result.RequiresTwoFactor) Console.WriteLine("[DEBUG] Login FAILED: 2FA Required.");
            
            Console.WriteLine("[DEBUG] Login FAILED: Invalid credentials.");
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }
        else
        {
            Console.WriteLine("[DEBUG] ModelState is INVALID.");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"[DEBUG] Validation Error: {error.ErrorMessage}");
            }
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        Console.WriteLine("[DEBUG] User Logged Out.");
        return RedirectToAction("Login", "Account");
    }
}
