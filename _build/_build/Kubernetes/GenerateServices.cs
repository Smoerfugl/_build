using k8s.Models;

namespace Build.Kubernetes;

public interface IGenerateServices
{
    List<V1Service> Invoke(IEnumerable<V1Deployment> deployments);
}

public class GenerateServices : IGenerateServices
{
    public List<V1Service> Invoke(IEnumerable<V1Deployment> deployments)
    {
        
        var services = deployments.Select(GenerateService);

        return services.ToList();
    }

    private static V1Service GenerateService(V1Deployment deployment)
    {
        var service = new V1Service()
        {
            Kind = "Service",
            ApiVersion = "v1",
            Metadata = new V1ObjectMeta()
            {
                NamespaceProperty = deployment.Namespace(),
                Name = deployment.Name(),
                Labels = deployment.Labels()
            },
            Spec = new V1ServiceSpec()
            {
                Selector = deployment.Labels(),
                Ports = new List<V1ServicePort>()
                {
                    new()
                    {
                        Name = "http",
                        Protocol = "TCP",
                        Port = deployment.Spec.Template.Spec.Containers.First().Ports.First().ContainerPort,
                        TargetPort = deployment.Spec.Template.Spec.Containers.First().Ports.First().ContainerPort
                    }
                }
            }
        };

        return service;
    }
}
