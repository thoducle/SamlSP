using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SamlSP.Models; // Import the model
using System.Security.Claims;

namespace SamlSP.Controllers
{
    public class SamlController : Controller
    {
        [HttpGet("saml/login")]
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/saml/claims" }, "Saml2");
        }

        [Authorize]
        [HttpGet("saml/claims")]
        public IActionResult Claims()
        {
            var claims = User.Claims
                .Select(c => new UserClaim { Type = c.Type, Value = c.Value })
                .ToList();

            return View(claims);
        }

        [HttpGet("saml/logout")]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties { RedirectUri = "/" }, "Saml2", "Cookies");
        }
    }
}
