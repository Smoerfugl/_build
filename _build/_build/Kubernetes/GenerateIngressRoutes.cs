using Build.Extensions;
using Build.Kubernetes.Builders;

namespace Build.Kubernetes;

public interface IGenerateIngressRoutes
{
    IEnumerable<IngressRoute> Invoke(Pipelines.Pipeline pipelineObject);
}

public class GenerateIngressRoutes : IGenerateIngressRoutes
{
    public IEnumerable<IngressRoute> Invoke(Pipelines.Pipeline pipelineObject)
    {
        var @namespace = @Namespace;
        var web = new IngressRouteBuilder()
            .WithName(Name)
            .WithPort(Port)
            .WithEntrypoint(Entrypoint.Web)
            .WithNamespace(@Namespace)
            .WithHostname(Hostname)
            .When(pipelineObject.RedirectHttps,
                builder => builder.WithMiddlewares(IngressRouteMiddleware.RedirectHttps())
            )
            .Build();
        if (!pipelineObject.Https)
        {
            return new List<IngressRoute>()
            {
                web
            };
        }

        var secure = new IngressRouteBuilder()
            .WithName(Name)
            .WithServiceName(ServiceName)
            .WithPort(Port)
            .WithEntrypoint(Entrypoint.WebSecure)
            .WithNamespace(@namespace)
            .WithHostname(Hostname)
            .Build();
        return new List<IngressRoute>()
        {
            web,
            secure
        };
    }

    public string ServiceName { get; set; } = null!;

    public string Hostname { get; set; } = null!;

    public string Namespace { get; set; } = null!;

    public int Port { get; set; }

    public string Name { get; set; } = null!;
}