using ChatRooms.BFF.Services;
using ChatRooms.BFF.Transforms;
using ChatRooms.DTOs.Users;
using ChatRooms.ServiceDefaults;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var keycloakBaseUrl = builder.Configuration["Keycloak:Authority"]
    ?.Replace("/realms/chatrooms", "")
    ?? "http://localhost:8080";

builder.Services.AddHttpClient<IKeycloakAdminService, KeycloakAdminService>(client =>
{
    client.BaseAddress = new Uri(keycloakBaseUrl);
});

builder.Services.AddHttpClient<KeycloakTokenService>(client =>
{
    client.BaseAddress = new Uri(keycloakBaseUrl);
});

builder.Services
    .AddHttpClient("keycloak-token", client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Keycloak:Authority"]
                ?.Replace("/realms/chatrooms", "")
            ?? "http://localhost:8080");
    });
builder.Services.AddScoped<KeycloakTokenService>();

builder.Services
    .AddHttpClient("chatrooms-api", client =>
    {
        client.BaseAddress = new Uri("https+http://chatrooms-api");
    });

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddOpenIdConnect(options =>
    {
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
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));

    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Too many requests. Please slow down." }, token);
    };
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver()
    .AddTransforms<BearerTokenTransformProvider>();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();


app.MapPost("/bff/register", async (
    RegisterBffRequest request,
    IKeycloakAdminService keycloakAdmin,
    KeycloakTokenService tokenService,
    IHttpClientFactory httpClientFactory,
    ILogger<Program> logger,
    CancellationToken ct) =>
{

    var adminToken = await tokenService.GetServiceAccountTokenAsync(ct);

    var keycloakUserId = await keycloakAdmin.CreateUserAsync(
        request, adminToken, ct);


    using var apiHttp = httpClientFactory.CreateClient("chatrooms-api");
    var apiResponse = await apiHttp.PostAsJsonAsync("/api/users/register",
        new
        {
            request.Name,
            request.Email,
            request.Password,
            request.Gender,
            request.BirthDate
        }, ct);

    if (!apiResponse.IsSuccessStatusCode)
    {
        await keycloakAdmin.DeleteUserAsync(keycloakUserId, adminToken, ct);

        return Results.Problem(
            detail: "Failed to create user account.",
            statusCode: StatusCodes.Status500InternalServerError);
    }

    var domainUser = await apiResponse.Content
        .ReadFromJsonAsync<UserDto>(ct);

    await keycloakAdmin.SetUserAttributeAsync(
        keycloakUserId, "systemuserid", domainUser!.Id.ToString(),
        adminToken, ct);

    if (logger.IsEnabled(LogLevel.Information))
        logger.LogInformation(
            "Registration complete. Keycloak: {KcId} → Domain: {DomainId}",
            keycloakUserId, domainUser.Id);

    return Results.Ok(new { message = "Account created. Please sign in." });

}).RequireRateLimiting("auth");

app.MapGet("/bff/login", async () => Results.Challenge(
    new AuthenticationProperties
    {
        RedirectUri = "/rooms",
        IsPersistent = true
    },
    [OpenIdConnectDefaults.AuthenticationScheme]))
    .RequireRateLimiting("auth");

app.MapGet("/bff/logout", async ctx =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = "/" });
}).RequireAuthorization();

app.MapReverseProxy()
   .RequireAuthorization();

await app.RunAsync();