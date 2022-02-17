﻿/* written and directed by Ingo Karstein 
https://github.com/ikarstein/com.kenaro.public.OAuth2Demo.Authentik

License: Apache 2

Please keep this comment.
*/

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace com.kenaro.@public.OAuth2Demo.Authentik;

public class AuthenticationController : Controller
{
    [HttpGet("~/signin")]
    [HttpPost("~/signin")]
    public IActionResult SignIn()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "authentik");
    }

    [HttpGet("~/signout")]
    [HttpPost("~/signout")]
    public IActionResult SignOutCurrentUser()
    {
        return SignOut(new AuthenticationProperties { RedirectUri = "/" }, CookieAuthenticationDefaults.AuthenticationScheme);
    }
}

