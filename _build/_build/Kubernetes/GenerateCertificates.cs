using Build.Kubernetes.Builders;

namespace Build.Kubernetes;

public interface IGenerateCertificates
{
    IEnumerable<CertManagerCertificate> Invoke(IEnumerable<IngressRoute> ingressRoutes);
}

public class GenerateCertificates : IGenerateCertificates
{
    public IEnumerable<CertManagerCertificate> Invoke(IEnumerable<IngressRoute> ingressRoutes)
    {
        var routes = GetHttpsRoutes(ingressRoutes);
        var certificates = CreateCertificates(routes);
        return certificates;
    }
    
    private static IEnumerable<IngressRoute> GetHttpsRoutes(IEnumerable<IngressRoute> ingressRoutes)
    {
        var routes = ingressRoutes.Where(d =>
            d.Spec.EntryPoints.Contains(
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