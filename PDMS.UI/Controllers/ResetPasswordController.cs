using Microsoft.AspNetCore.Mvc;
using PDMS.Application.Interfaces;
using PDMS.Domain.Entity;
using PDMS.UI.Models;
using Serilog;
using System;
using System.Threading.Tasks;


namespace PDMS.UI.Controllers
{
    public class ResetPasswordController : Controller
    {
        private readonly IResetPasswordService _resetPasswordService;

        public ResetPasswordController(IResetPasswordService resetPasswordService)
        {
            _resetPasswordService = resetPasswordService;
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPassword)
        {
            if (ModelState.IsValid)
            {
                if (!await _resetPasswordService.IsUsernameInUse(forgotPassword.Username))
                {
                    Log.Information("Couldnot proceed with reset password. User with the provided username {0} is not registered.", forgotPassword.Username);
                    ModelState.AddModelError("Username", "Username not registered.");
                    return View("ForgotPassword");
                }

                var randomToken = await _resetPasswordService.GenerateRandomToken();
                var passwordResetLink = Url.Action("Reset", "ResetPassword", new { email = forgotPassword.Username, code = randomToken }, "https");

                var resetPasswordServiceResponse = await _resetPasswordService.SendPasswordResetLink(forgotPassword.Username, passwordResetLink, randomToken);

                if (resetPasswordServiceResponse.ErrorMessage != null)
                {
                    ModelState.AddModelError("Username", "Unable to Send Password reset token.");
                    return View("ForgotPassword");
                }
                else
                {
                    TempData["Message"] = "Password reset link sent successfully!!";
                    return RedirectToAction("Index", "Login");
                }
            }
            else
                return View("ForgotPassword");
        }

        [HttpGet]
        public IActionResult Reset(string code, string email)
        {
            ResetPasswordModel resetPasswordModel = new ResetPasswordModel();
            resetPasswordModel.ReturnToken = code;
            resetPasswordModel.Username = email;
            return View("ResetPassword", resetPasswordModel);
        }

        [HttpPost]
        public async Task<IActionResult> Reset(ResetPasswordModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
            {
                return View("ResetPassword");
            }
            var resetPasswordServiceResponse = await _resetPasswordService.SetNewPassword(resetPasswordModel.Username, resetPasswordModel.ReturnToken, resetPasswordModel.NewPassword);

            if (resetPasswordServiceResponse.ErrorMessage != null)
            {
                ModelState.AddModelError("Username", resetPasswordServiceResponse.ErrorMessage);
                return View("ResetPassword");
            }
            else
            {
                TempData["Message"] = "Please login. Password changed successfully!!";
                return RedirectToAction("Index", "Login");
            }
        }
    }
}


