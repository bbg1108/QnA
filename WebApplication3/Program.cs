using WebApplication3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = "https://accounts.google.com";
    options.ClientId = builder.Configuration["Google:ClientId"];
    options.ClientSecret = builder.Configuration["Google:ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = context =>
        {
            var claims = context.Principal.Identity as ClaimsIdentity;

            if (claims != null)
            {
                claims.AddClaim(new Claim(ClaimTypes.Name, context.Principal.FindFirstValue(ClaimTypes.Name) ?? ""));
                claims.AddClaim(new Claim(ClaimTypes.Email, context.Principal.FindFirstValue(ClaimTypes.Email) ?? ""));
                var userEmail = claims.FindFirst(ClaimTypes.Email)?.Value;
                Console.WriteLine("User email: " + userEmail);

                if (userEmail == "bbg8080@gmail.com")
                {
                    claims.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                }
            }

            return Task.CompletedTask;
        }
    };
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<QnaViewModel>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
// 이메일 연결
builder.Services.AddScoped<GmailEmailService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var user = provider.GetRequiredService<IHttpContextAccessor>().HttpContext.User;
    return new GmailEmailService(configuration, user);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapGet("/info", (ClaimsPrincipal user) =>
{
    var userName = user.FindFirstValue(ClaimTypes.Name);
    var userEmail = user.FindFirstValue(ClaimTypes.Email);
    var userRoles = string.Join(", ", user.FindAll(ClaimTypes.Role).Select(r => r.Value));

    // Pass login information to the view
    return Results.Ok(new { userName, userEmail, userRoles });
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
