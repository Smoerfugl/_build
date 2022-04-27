using Build.Pipelines;
using Build.Environments;

namespace Build.Kubernetes;

public interface IGenerateManifesto
{
    List<object> Invoke(Pipeline pipeline, Env env, string domainValue);
}

public class GenerateManifesto : IGenerateManifesto
{
    public List<object> Invoke(Pipeline pipeline, Env env, string domainValue)
    {
        var routes = new GenerateIngressRoutesRoutesList().Invoke(pipeline, domainValue);
        var objects = new List<object>
        {
            new GenerateNamespace(pipeline, env).Invoke(),
            routes,
            new GenerateCertificates().Invoke(routes),
        };
        return objects;
    }
}