/* written and directed by Ingo Karstein 
https://github.com/ikarstein/com.kenaro.public.OAuth2Demo.Authentik

License: Apache 2

Please keep this comment.
*/

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace com.kenaro.@public.OAuth2Demo.Authentik;

public class HomeController : Controller
{
    [HttpGet("~/")]
    public ActionResult Index()
    {
        return View();
    }
}
