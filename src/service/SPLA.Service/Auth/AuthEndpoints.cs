using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SPLA.Service.Auth;

/// <summary>Maps the local-credentials pages and their POST handlers: sign-in, self-registration,
/// sign-out, and self-service password change. All of them issue or clear the same <c>spla.auth</c>
/// cookie every other auth mode uses, so the WebSocket upgrade and per-user areas work unchanged.</summary>
internal static class AuthEndpoints
{
    public static void Map(WebApplication app, LocalAccountService accounts)
    {
        app.MapGet("/login", (HttpContext ctx, string? returnUrl, string? error) =>
        {
            if (ctx.User.Identity?.IsAuthenticated == true)
                return Results.Redirect(SafeReturn(returnUrl));
            return Html(AuthPages.Login(returnUrl, error, accounts.AllowSelfRegistration));
        }).AllowAnonymous();

        app.MapPost("/login", async (HttpContext ctx) =>
        {
            var form = await ctx.Request.ReadFormAsync();
            var returnUrl = form["returnUrl"].ToString();
            var user = accounts.Validate(form["username"].ToString(), form["password"].ToString());
            if (user is null)
                return Results.Redirect($"/login?error={Uri.EscapeDataString("Invalid username or password, or the account is disabled.")}&returnUrl={Uri.EscapeDataString(SafeReturn(returnUrl))}");

            await SignInAsync(ctx, user);
            return Results.Redirect(SafeReturn(returnUrl));
        }).AllowAnonymous();

        app.MapGet("/register", (HttpContext ctx, string? returnUrl, string? error) =>
        {
            if (!accounts.AllowSelfRegistration) return Results.Redirect("/login");
            if (ctx.User.Identity?.IsAuthenticated == true) return Results.Redirect(SafeReturn(returnUrl));
            return Html(AuthPages.Register(returnUrl, error));
        }).AllowAnonymous();

        app.MapPost("/register", async (HttpContext ctx) =>
        {
            if (!accounts.AllowSelfRegistration) return Results.Redirect("/login");

            var form = await ctx.Request.ReadFormAsync();
            var returnUrl = form["returnUrl"].ToString();
            var result = accounts.Register(
                form["username"].ToString(), form["password"].ToString(), form["displayName"].ToString());
            if (!result.Ok)
                return Results.Redirect($"/register?error={Uri.EscapeDataString(result.Error!)}&returnUrl={Uri.EscapeDataString(SafeReturn(returnUrl))}");

            await SignInAsync(ctx, result.User!);
            return Results.Redirect(SafeReturn(returnUrl));
        }).AllowAnonymous();

        // Where the cookie handler sends an authenticated-but-forbidden browser navigation (a non-admin
        // hitting /admin). API paths get a 403 status instead (see the cookie events).
        app.MapGet("/Account/AccessDenied", () =>
            Results.Content(AuthPages.AccessDenied(), "text/html; charset=utf-8", statusCode: StatusCodes.Status403Forbidden));

        app.MapGet("/logout", async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/login");
        });

        // Self-service account page (any signed-in user).
        app.MapGet("/account", (HttpContext ctx, string? notice) =>
        {
            var key = ctx.User.FindFirst(AuthClaims.UserKey)?.Value;
            var user = key is null ? null : accounts.FindByKey(key);
            if (user is null) return Results.Redirect("/login");
            return Html(AuthPages.Account(user.DisplayName, user.Username, notice));
        });

        app.MapPost("/account/password", async (HttpContext ctx) =>
        {
            var key = ctx.User.FindFirst(AuthClaims.UserKey)?.Value;
            if (key is null) return Results.Redirect("/login");

            var form = await ctx.Request.ReadFormAsync();
            if (!accounts.VerifyPassword(key, form["current"].ToString()))
                return Results.Redirect($"/account?notice={Uri.EscapeDataString("Current password is incorrect.")}");

            var result = accounts.SetPassword(key, form["next"].ToString());
            var msg = result.Ok ? "Password updated." : result.Error!;
            return Results.Redirect($"/account?notice={Uri.EscapeDataString(msg)}");
        });
    }

    /// <summary>Builds the cookie principal from a validated user: the owner key, display name, role
    /// claims (for the admin policy), and group claims (carried into the connection identity).</summary>
    internal static async Task SignInAsync(HttpContext ctx, LocalUser user)
    {
        var claims = new List<Claim>
        {
            new(AuthClaims.UserKey, user.UserKey),
            new(ClaimTypes.Name, user.DisplayName)
        };
        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(user.Groups.Select(g => new Claim(AuthClaims.Group, g)));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    private static IResult Html(string html) => Results.Text(html, "text/html; charset=utf-8");

    /// <summary>Only ever redirect to a local path — never an absolute URL supplied in a query string
    /// (open-redirect guard).</summary>
    private static string SafeReturn(string? returnUrl)
        => !string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//")
            ? returnUrl : "/";
}
