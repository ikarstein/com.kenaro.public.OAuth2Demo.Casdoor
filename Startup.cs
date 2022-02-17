/* written and directed by Ingo Karstein 
https://github.com/ikarstein/com.kenaro.public.OAuth2Demo.Casdoor

License: Apache 2

Please keep this comment.
*/

using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace com.kenaro.@public.OAuth2Demo.Casdoor;

public class Startup
{
    public Startup(IConfiguration configuration, IHostEnvironment hostingEnvironment)
    {
        Configuration = configuration;
        HostingEnvironment = hostingEnvironment;
    }

    public IConfiguration Configuration { get; }

    private IHostEnvironment HostingEnvironment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRouting();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })

        .AddCookie(options =>
        {
            options.LoginPath = "/signin";
            options.LogoutPath = "/signout";
        })
        .AddOAuth("casdoor", "Casdoor", options =>
        {
            options.AuthorizationEndpoint = "http://localhost:8000/login/oauth/authorize";
            options.TokenEndpoint = "http://localhost:8000/api/login/oauth/access_token";
            options.UserInformationEndpoint = "http://localhost:8000/api/userinfo";
            options.ClientId = "dc6556419364997a4032";
            options.ClientSecret = "2a4dbd07bbb655777a928ef99039a11d1e81d9d4";
            options.CallbackPath = "/signin-casdoor";
            options.ClaimsIssuer = "iss";
            options.SaveTokens = true;
            options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "name");
            options.ClaimActions.MapJsonSubKey(ClaimTypes.Gender, "data", "gender");
            options.ClaimActions.MapJsonSubKey(ClaimTypes.Name, "data", "displayName");
            options.ClaimActions.MapJsonSubKey(ClaimTypes.Email, "data", "email");
            options.ClaimActions.MapJsonSubKey(ClaimTypes.HomePhone, "data", "phone");
            options.ClaimActions.MapJsonSubKey(ClaimTypes.Locality, "data", "location");
            options.ClaimActions.MapJsonSubKey(ClaimTypes.Webpage, "data", "homepage");
            options.ClaimActions.MapJsonSubKey(ClaimTypes.Role, "data", "type");

            options.Events.OnCreatingTicket = async creatingTicketContext =>
            {
                var token = creatingTicketContext.Properties?.GetString(".Token.access_token");

                using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8000/api/get-account");
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                using var response = await creatingTicketContext.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, creatingTicketContext.HttpContext.RequestAborted);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("An error occurred while retrieving the user profile from Authentik.");
                }

                var userInfo = await response.Content.ReadAsStringAsync(creatingTicketContext.HttpContext.RequestAborted);
                using var jsonDoc = JsonDocument.Parse(userInfo);
               
                creatingTicketContext.RunClaimActions(jsonDoc.RootElement);
            };
        });

        services.AddMvc();
    }

    public void Configure(IApplicationBuilder app)
    {
        if (HostingEnvironment.IsDevelopment())
        {
        }

        var options = new StaticFileOptions()
        {
            ServeUnknownFileTypes = true,
        };

        app.UseStaticFiles(options);

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
        });
    }
}
