using Microsoft.AspNetCore.Mvc;
using PDMS.Application.Interfaces;
using PDMS.Domain.Entity;
using PDMS.UI.Models;
using Serilog;
using System;
using System.Threading.Tasks;

namespace PDMS.UI.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly IAuthenticationService _authenticationService;

        public RegistrationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View("SignUpForm");
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(Registration registration)
        {
            if (ModelState.IsValid)
            {
                var authenticationResponse =
                    await _authenticationService.CreateUser(registration.FirstName, registration.LastName, registration.EmailId, registration.Password);

                if (authenticationResponse.IsSuccessful())
                {
                    Log.Information("New Registration Successful for {0}", registration.EmailId);
                    return RedirectToAction("Index", "Login");
                }

                ModelState.AddModelError("ConfirmPassword", authenticationResponse.ErrorMessage);
                Log.Information("New Registration failed for {0}", registration.EmailId);
                return RedirectToAction("Index", "Login");
            }
            return View("SignUpForm");
        }
    }
}