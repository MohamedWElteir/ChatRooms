using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace ChatRooms.BFF.Transforms;

public sealed class BearerTokenTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context) { }
    public void ValidateCluster(TransformClusterValidationContext context) { }

    public void Apply(TransformBuilderContext context)
    {
        var bearerTokenTransform = new BearerTokenTransform();
        context.AddRequestTransform(async transformContext =>
        {
            await bearerTokenTransform.ApplyAsync(transformContext);
        });
    }
}