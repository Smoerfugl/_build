using Build.Kubernetes.Builders;

namespace Build.Kubernetes;

public interface IGenerateIngressRoutes
{
    IEnumerable<IngressRoute> Invoke();
}

public class GenerateIngressRoutes : IGenerateIngressRoutes
{
    public IEnumerable<IngressRoute> Invoke()
    {
        var @namespace = @Namespace;
        var web = new IngressRouteBuilder()
            .WithName(Name)
            .WithPort(Port)
            .WithEntrypoint(Entrypoint.Web)
            .WithNamespace(@Namespace)
            .WithHostname(Hostname)
            .WithMiddlewares(IngressRouteMiddleware.RedirectHttps())
            .Build();
        var secure = new IngressRouteBuilder()
            .WithName(Name)
            .WithServiceName(ServiceName)
            .WithPort(Port)
            .WithEntrypoint(Entrypoint.Secure)
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