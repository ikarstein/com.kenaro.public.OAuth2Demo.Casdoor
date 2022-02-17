/* written and directed by Ingo Karstein 
https://github.com/ikarstein/com.kenaro.public.OAuth2Demo.Casdoor

License: Apache 2

Please keep this comment.
*/

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace com.kenaro.@public.OAuth2Demo.Casdoor;

public class HomeController : Controller
{
    [HttpGet("~/")]
    public ActionResult Index()
    {
        return View();
    }
}
