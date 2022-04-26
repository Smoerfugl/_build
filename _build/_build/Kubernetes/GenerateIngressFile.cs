using Build.Tye;

namespace Build.Kubernetes;

public interface IGenerateIngressFile
{
    List<object> Invoke();
}

public class GenerateIngressFile : IGenerateIngressFile
{
    private readonly string _domainValue;
    public TyeConfigGenerator TyeObject { get; }
    public Pipeline.Pipeline PipelineObject { get; }

    public GenerateIngressFile(TyeConfigGenerator tyeObject, Pipeline.Pipeline pipelineObject, string domainValue)
    {
        _domainValue = domainValue;
        TyeObject = tyeObject;
        PipelineObject = pipelineObject;
    }

    public List<object> Invoke()
    {
        var objects = new List<object>();
        var ingressRoutes = GetIngressRoutes();

        var routes = GetHttpsRoutes(ingressRoutes);
        objects.AddRange(ingressRoutes);

        var certificates = CreateCertificates(routes);
        objects.AddRange(certificates);

        objects.Add(new GenerateNamespace(TyeObject.Namespace).Invoke());

        return objects;
    }

    private List<IngressRoute> GetIngressRoutes()
    {
        var ingressRoutes = new List<IngressRoute>();
        PipelineObject.Services.ForEach(service =>
        {
            var tyeService = TyeObject.Services.SingleOrDefault(d => d.Name == service.Name);
            if (tyeService == null)
            {
                return;
            }

            if (service.ServicePort == null)
            {
                return;
            }

            var routes = new GenerateIngressRoutes()
            {
                Name = tyeService.Name,
                Hostname = $"{service.Hostname}.{_domainValue}",
                Namespace = TyeObject.Namespace,
                Port = service.ServicePort.Value,
                ServiceName = tyeService.Name
            }.Invoke();

            ingressRoutes.AddRange(routes);
        });
        return ingressRoutes;
    }

    private static IEnumerable<IngressRoute> GetHttpsRoutes(IEnumerable<IngressRoute> ingressRoutes)
    {
        var routes = ingressRoutes.Where(d =>
            d.Spec.Entrypoints.Contains(
                Entrypoint.Secure.ToString().ToLower()
            )
        ).ToList();
        return routes;
    }

    private static IEnumerable<CertManagerCertificate> CreateCertificates(IEnumerable<IngressRoute> routes)
    {
        var certificates = routes
            .Where(d => d.Spec.Tls != null)
            .Select(CreateCert)
            .ToList();
        return certificates;
    }

    private static CertManagerCertificate CreateCert(IngressRoute route)
    {
            var cert = new CertManagerCertificateBuilder()
                .WithName(route.Metadata.Name)
                .WithNamespace(route.Metadata.NamespaceProperty)
                .WithDnsNames(route.Spec.Routes.Select(d => d.GetRawHostname()).ToArray())
                .WithSecretName(route.Spec.Tls!.SecretName)
                .Build();

            return cert;
    }
}