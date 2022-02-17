/* written and directed by Ingo Karstein 
https://github.com/ikarstein/com.kenaro.public.OAuth2Demo.Authentik

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

namespace com.kenaro.@public.OAuth2Demo.Authentik;

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
        .AddOAuth("authentik", "Authentik", options =>
        {
            options.AuthorizationEndpoint = "https://auth.kenaro.com/application/o/authorize/";
            options.TokenEndpoint = "https://auth.kenaro.com/application/o/token/";
            options.UserInformationEndpoint = "https://auth.kenaro.com/application/o/userinfo/";
            options.ClientId = "514953ed27fe39c035723759b5c329c9a338945e";
            options.ClientSecret = "392fca0ff7be298900b84e82af0b2f7cfe487a9963404b28c252fc9524a345b8ee53506fb7d51f824dbc6045077780f3b1c85eba204b626a49213212bfa877c0";
            options.CallbackPath = "/signin-authentik";
            options.ClaimsIssuer = "Authentik";
            options.SaveTokens = true;

            options.Scope.Add("email");
            options.Scope.Add("openid");
            options.Scope.Add("username");
            options.Scope.Add("openid");
            options.Scope.Add("profile");
           
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
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://auth.kenaro.com/application/o/userinfo");
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(creatingTicketContext.TokenType, creatingTicketContext.AccessToken);

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
