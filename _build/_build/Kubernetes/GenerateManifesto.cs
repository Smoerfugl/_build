using Build.Pipelines;
using Environment = Build.Environments.Environment;

namespace Build.Kubernetes;

public interface IGenerateManifesto
{
    List<object> Invoke(Pipeline pipeline);
}

public class GenerateManifesto : IGenerateManifesto
{
    private readonly IGenerateNamespace _generateNamespace;
    private readonly IGenerateCertificates _generateCertificates;
    private readonly IGenerateIngressRoutesList _generateIngressRoutesList;
    private readonly IDomain _domain;

    public GenerateManifesto(IGenerateNamespace generateNamespace, 
        IGenerateCertificates generateCertificates,
        IGenerateIngressRoutesList generateIngressRoutesList,
        IDomain domain)
    {
        _generateNamespace = generateNamespace;
        _generateCertificates = generateCertificates;
        _generateIngressRoutesList = generateIngressRoutesList;
        _domain = domain;
    }
    public List<object> Invoke(Pipeline pipeline)
    {
        var routes = _generateIngressRoutesList.Invoke(pipeline, _domain.Value);
        var objects = new List<object>
        {
            _generateNamespace.Invoke(),
            routes,
            _generateCertificates.Invoke(routes),
        };
        return objects;
    }
}