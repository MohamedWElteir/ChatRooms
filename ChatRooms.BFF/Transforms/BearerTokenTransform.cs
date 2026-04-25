using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace ChatRooms.BFF.Transforms;

public sealed class BearerTokenTransform : RequestTransform
{
    public override async ValueTask ApplyAsync(RequestTransformContext context)
    {
        var token = await context.HttpContext
            .GetTokenAsync("access_token");

        if (!string.IsNullOrWhiteSpace(token))
        {
            context.ProxyRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}