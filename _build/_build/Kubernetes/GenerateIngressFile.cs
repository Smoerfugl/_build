using Build.Tye;

namespace Build.Kubernetes;

public class GenerateIngressFile
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

    public List<Object> Invoke()
    {
        var objects = new List<object>();
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

        foreach (var route in ingressRoutes.Where(d => d.Spec.Entrypoints.Contains(Entrypoint.Secure.ToString().ToLower()) ))
        {
            if (route.Spec.Tls == null)
            {
                continue;
            }
            
            var cert = new CertManagerCertificateBuilder()
                .WithName(route.Metadata.Name)
                .WithNamespace(route.Metadata.NamespaceProperty)
                .WithDnsNames(route.Spec.Routes.Select(d => d.GetRawHostname()).ToArray())
                .WithSecretName(route.Spec.Tls.SecretName)
                .Build();
            objects.Add(cert);
        }
        
        objects.AddRange(ingressRoutes);
        objects.Add(new GenerateNamespace(TyeObject.Namespace).Invoke());

        return objects;
    }
}