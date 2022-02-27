using Microsoft.AspNetCore.Mvc;
using PDMS.Application.Interfaces;
using PDMS.UI.Models;
using PDMS.Domain.Entity;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using PDMS.UI.Attributes;
using Serilog;

namespace PDMS.UI.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAuthenticationService _authenticationService;

        public LoginController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (String.IsNullOrEmpty(HttpContext.Session.GetString("username")))
            {
                ViewBag.Message = TempData["Message"];
                return View("LoginForm");
            }    
            ViewBag.Username = HttpContext.Session.GetString("username");   
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginModel login)
        {
            if (!ModelState.IsValid)
                return View("LoginForm");

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var loginResult = await _authenticationService.Login(login.EmailId, login.Password, ipAddress);

            if (!loginResult.IsSuccessful())
            {
                ModelState.AddModelError("Password", loginResult.ErrorMessage);
                return View("LoginForm");
            }

            AddSessionCookie(loginResult.SessionId, login.EmailId);

            HttpContext.Session.SetString("username", login.EmailId);
            HttpContext.Session.SetString("sessionTimeOut", "15");
            ViewBag.Username = login.EmailId;

            Log.Information("User {0} from {1} logged on to the system.", login.EmailId, ipAddress);
            return RedirectToAction("Index", "Home");
        }

        private void AddSessionCookie(string sessionId, string username)
        {
            Response.Cookies.Append("SessionId", sessionId);
            Response.Cookies.Append("Username", username);
        }

        [RequireSession]
        public async Task<IActionResult> Logout()
        {
            var currentSessionId = Request.Cookies["SessionId"];
            var logoutResult = await _authenticationService.Logout(currentSessionId);

            if (!logoutResult.IsSuccessful())
            {
                ModelState.AddModelError("", logoutResult.ErrorMessage);
                Log.Information("User {0} log off unsuccessful.", Request.Cookies["Username"]);
                return View("LoginForm");
            }
            Log.Information("User {0} log off successful.", Request.Cookies["Username"]);
            if (Request.Cookies["Username"] != null)
            {
                Response.Cookies.Delete("Username");
                Response.Cookies.Delete("SessionId");
            }
            return View("LoginForm");
        }
    }
}
