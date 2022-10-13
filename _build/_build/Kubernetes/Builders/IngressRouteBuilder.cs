using k8s.Models;

namespace Build.Kubernetes.Builders;

public class IngressRouteBuilder
{
    private string _hostname = null!;
    private string _name = null!;
    private int _port;
    private Entrypoint _entrypoint;
    private List<IngressRouteMiddleware>? _middleware;
    private IDictionary<string,string> _labels = new Dictionary<string, string>();
    private string _namespace = null!;

    public IngressRouteBuilder WithHostname(string host)
    {
        _hostname = host;
        return this;
    }

    public IngressRouteBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public IngressRouteBuilder WithNamespace(string @namespace)
    {
        _namespace = @namespace;
        return this;
    }

    public IngressRouteBuilder WithServiceName(string serviceName)
    {
        return this;
    }

    public IngressRouteBuilder WithPort(int port)
    {
        _port = port;
        return this;
    }

    public IngressRouteBuilder WithEntrypoint(Entrypoint entrypoint)
    {
        _entrypoint = entrypoint;
        return this;
    }

    public IngressRouteBuilder WithMiddlewares(params IngressRouteMiddleware[] middleware)
    {
        _middleware = middleware.ToList();
        return this;
    }

    public IngressRoute Build()
    {
        _labels.Add("app.kubernetes.io/name", _name);
        _labels.Add("app.kubernetes.io/part-of", _namespace);
        return new IngressRoute()
        {
            Metadata = new V1ObjectMeta()
            {
                Name = _name,
                Labels = _labels,
                NamespaceProperty = _namespace
            },
            Spec = new IngressRouteSpec()
            {
                EntryPoints = new List<string>()
                {
                    _entrypoint.ToString().ToLower()
                },
                Routes = new List<IngressRouteRule>()
                {
                    new IngressRouteRule($"Host(`{_hostname}`)")
                    {
                        Services = new List<IngressRouteService>()
                        {
                            new IngressRouteService()
                            {
                                Name = _name,
                                Port = _port
                            }
                        },
                        Middlewares = _middleware?.ToList()
                    }
                },
                Tls = _entrypoint == Entrypoint.WebSecure ? new IngressRouteSpecTls(_hostname) : null
            }
        };
    }
}