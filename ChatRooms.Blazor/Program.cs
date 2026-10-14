using System.Security.Claims;
using ChatRooms.Blazor;
using ChatRooms.Blazor.Components;
using ChatRooms.Blazor.HttpClients;
using ChatRooms.ServiceDefaults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddBlazorServices();
builder.Services.AddSingleton<ServerSessionStore>();
builder.Services.AddScoped<UserContext>();
builder.Services.AddScoped<CircuitHandler, UserContextCircuitHandler>();
builder.Services.AddSingleton<CircuitServicesAccessor>();
builder.Services.AddScoped<CircuitHandler, ServicesAccessorCircuitHandler>();
builder.Services.AddScoped<AccessTokenRefresher>();
builder.Services.AddHttpClient("keycloak", client =>
{
    var authority = builder.Configuration["Keycloak:Authority"];
    client.BaseAddress = new Uri(
        authority is null
            ? throw new InvalidOperationException("Keycloak:Authority is not configured.")
            : new Uri(authority).GetLeftPart(UriPartial.Authority));
});
builder.Services.AddTransient<AuthorizationDelegatingHandler>();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Authority = builder.Configuration["Keycloak:Authority"];
    options.MetadataAddress = builder.Configuration["Keycloak:MetadataAddress"];
    options.ClientId = builder.Configuration["Keycloak:ClientId"];
    options.ClientSecret = builder.Configuration["Keycloak:ClientSecret"];
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.UsePkce = true;
    options.SaveTokens = true;
    options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.GetClaimsFromUserInfoEndpoint = true;
    options.MapInboundClaims = false;
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("roles");
    options.TokenValidationParameters.ValidateIssuer = true;
    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = context =>
        {
            var sub = context.Principal?.FindFirst("sub")?.Value
                ?? context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(sub) && context.TokenEndpointResponse is not null)
            {
                var sessionStore = context.HttpContext.RequestServices.GetRequiredService<ServerSessionStore>();
                var accessToken = context.TokenEndpointResponse.AccessToken;
                var refreshToken = context.TokenEndpointResponse.RefreshToken;
                DateTimeOffset? expiresAt = int.TryParse(context.TokenEndpointResponse.ExpiresIn, out var exp)
                    ? DateTimeOffset.UtcNow.AddSeconds(exp)
                    : null;
                sessionStore.Store(sub, accessToken, refreshToken, expiresAt);
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", () => Results.Challenge(
    new AuthenticationProperties { RedirectUri = "/rooms" },
    [OpenIdConnectDefaults.AuthenticationScheme]));

app.MapGet("/logout", async (HttpContext ctx, ServerSessionStore sessionStore) =>
{
    var sub = ctx.User.FindFirst("sub")?.Value ?? ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!string.IsNullOrEmpty(sub))
    {
        sessionStore.Remove(sub);
    }
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = "/" });
}).RequireAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
