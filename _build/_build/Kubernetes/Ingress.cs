using k8s.Models;

namespace Build.Kubernetes;

public class Namespace
{
    public string ApiVersion { get; set; } = "v1";
    public string Kind { get; set; } = "Namespace";
    public Dictionary<object, string> Metadata { get; set; } = new();
}

public class IngressRoute
{
    public string ApiVersion { get; set; } = "traefik.containo.us/v1alpha1";
    public string Kind { get; set; } = "IngressRoute";
    public V1ObjectMeta Metadata { get; set; } = new();
    public IngressRouteSpec Spec { get; set; } = new();
}

public class IngressRouteSpec
{
    public List<string> Entrypoints { get; set; } = new();
    public List<IngressRouteRule> Routes { get; set; } = new();
    public IngressRouteSpecTls? Tls { get; set; }
}

public class IngressRouteRule
{
    public IngressRouteRule(string match)
    {
        Match = match;
    }

    public string Match { get; set; }
    public string Kind { get; set; } = "Rule";
    public List<IngressRouteService> Services { get; set; } = new() { };
    public List<IngressRouteMiddleware>? Middlewares { get; set; }
    public IngressRouteSpecTls Tls { get; set; } = null!;
    public string GetRawHostname() => Match.Replace("Host(`","").Replace("`)","");

}

public class IngressRouteService
{
    public string Name { get; set; } = null!;
    public int Port { get; set; }
}

public class IngressRouteMiddleware
{
    public IngressRouteMiddleware(string name, string ns)
    {
        Name = name;
        Namespace = ns;
    }

    public static IngressRouteMiddleware RedirectHttps()
    {
        return new IngressRouteMiddleware("redirect-https", "default");
    }

    public string Name { get; set; }
    public string Namespace { get; set; }
}

public class IngressRouteSpecTls
{
    public IngressRouteSpecTls(string secretName)
    {
        SecretName = secretName;
    }

    public string SecretName { get; set; }
}
