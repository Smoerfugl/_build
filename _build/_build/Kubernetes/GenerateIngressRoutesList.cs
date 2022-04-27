using Build.Environments;

namespace Build.Kubernetes;

public interface IGenerateIngressRoutesList
{
    List<IngressRoute> Invoke(Pipelines.Pipeline pipelineObject, string domainValue);
}

public class GenerateIngressRoutesRoutesList : IGenerateIngressRoutesList
{
    public List<IngressRoute> Invoke(Pipelines.Pipeline pipelineObject, string domainValue)
    {
        var ingressRoutes = GetIngressRoutes(pipelineObject, domainValue);
        return ingressRoutes;
    }

    private List<IngressRoute> GetIngressRoutes(Pipelines.Pipeline pipelineObject,
        string domainValue)
    {
        var ingressRoutes = new List<IngressRoute>();
        pipelineObject.Services.ForEach(service =>
        {
            if (service.Hostname == null)
            {
                return;
            }

            var routes = new GenerateIngressRoutes()
            {
                Name = service.Name,
                Hostname = $"{service.Hostname}.{domainValue}",
                Namespace = pipelineObject.Name,
                Port = service.ServicePort,
                ServiceName = service.Name
            }.Invoke();

            ingressRoutes.AddRange(routes);
        });
        return ingressRoutes;
    }
}