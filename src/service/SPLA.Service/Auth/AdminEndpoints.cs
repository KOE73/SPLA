using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SPLA.Service.Auth;

/// <summary>The admin panel: an HTML page plus a small JSON REST API for user management (list,
/// create, edit, reset password, enable/disable, delete). Everything here requires the <c>admin</c>
/// role via the <c>spla.admin</c> authorization policy, so an ordinary signed-in user cannot reach it.</summary>
internal static class AdminEndpoints
{
    private const string AdminPolicy = "spla.admin";

    public static void Map(WebApplication app, LocalAccountService accounts)
    {
        app.MapGet("/admin", (HttpContext ctx) =>
            Results.Text(AuthPages.Admin(ctx.User.Identity?.Name ?? "admin"), "text/html; charset=utf-8"))
            .RequireAuthorization(AdminPolicy);

        var api = app.MapGroup("/admin/api").RequireAuthorization(AdminPolicy);

        api.MapGet("/users", () => Results.Json(accounts.ListUsers().Select(ToDto)));

        api.MapPost("/users", (UserCreateDto dto) =>
        {
            var result = accounts.Register(dto.Username ?? "", dto.Password ?? "", dto.DisplayName, dto.Roles, dto.Groups);
            return result.Ok ? Results.Json(ToDto(result.User!)) : Bad(result.Error);
        });

        api.MapPut("/users/{key}", (string key, UserUpdateDto dto) =>
        {
            if (accounts.FindByKey(key) is null) return Bad("User not found.", StatusCodes.Status404NotFound);

            // Apply each facet; the first refusal (e.g. last-admin guard) wins and is reported.
            if (dto.DisplayName is not null)
            {
                var r = accounts.SetProfile(key, dto.DisplayName);
                if (!r.Ok) return Bad(r.Error);
            }
            if (dto.Roles is not null)
            {
                var r = accounts.SetRoles(key, dto.Roles);
                if (!r.Ok) return Bad(r.Error);
            }
            if (dto.Groups is not null)
            {
                var r = accounts.SetGroups(key, dto.Groups);
                if (!r.Ok) return Bad(r.Error);
            }
            if (!string.IsNullOrEmpty(dto.Password))
            {
                var r = accounts.SetPassword(key, dto.Password);
                if (!r.Ok) return Bad(r.Error);
            }

            return Results.Json(ToDto(accounts.FindByKey(key)!));
        });

        api.MapPost("/users/{key}/enabled", (string key, EnabledDto dto) =>
        {
            var r = accounts.SetEnabled(key, dto.Enabled);
            return r.Ok ? Results.Json(ToDto(r.User!)) : Bad(r.Error);
        });

        api.MapDelete("/users/{key}", (string key) =>
        {
            var r = accounts.Delete(key);
            return r.Ok ? Results.Ok() : Bad(r.Error);
        });
    }

    private static IResult Bad(string? error, int status = StatusCodes.Status400BadRequest)
        => Results.Json(new { error = error ?? "Failed." }, statusCode: status);

    private static object ToDto(LocalUser u) => new
    {
        u.UserKey,
        u.Username,
        u.DisplayName,
        u.Roles,
        u.Groups,
        u.Enabled,
        u.CreatedUtc,
        u.LastLoginUtc
    };

    private sealed record UserCreateDto(string? Username, string? Password, string? DisplayName, List<string>? Roles, List<string>? Groups);
    private sealed record UserUpdateDto(string? DisplayName, string? Password, List<string>? Roles, List<string>? Groups);
    private sealed record EnabledDto(bool Enabled);
}
