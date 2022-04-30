using Build.Environments;
using Build.Kubernetes.Builders;
using Build.Pipelines;
using k8s.Models;

namespace Build.Kubernetes;

public interface IGenerateDeployments
{
    IEnumerable<V1Deployment> Invoke(Pipeline pipelines, Env env);
}

public class GenerateDeployments : IGenerateDeployments
{
    public IEnumerable<V1Deployment> Invoke(Pipeline pipeline, Env env)
    {
        var ns = pipeline.GetNamespace(env);

        var key = pipeline.EnvironmentVariables.Keys.SingleOrDefault(d =>
            string.Equals(d, env.ToString(), StringComparison.CurrentCultureIgnoreCase));

        var deployments = pipeline.Services.Select(d =>
            GenerateDeployment(d, ns, pipeline.EnvironmentVariables[key])
        );
        return deployments;
    }
    
    private V1Deployment GenerateDeployment(PipelineService pipelineService, string ns,
        List<EnvironmentVariable> environmentVariables)
    {
        var deployment = new V1Deployment()
        {
            Metadata = new V1ObjectMeta()
            {
                Name = pipelineService.Name,
                NamespaceProperty = ns,
                Labels = new Dictionary<string, string>()
                {
                    { "app.kubernetes.io/name", pipelineService.Name },
                    { "app.kubernetes.io/part-of", ns }
                },
            },
            Spec = new V1DeploymentSpec()
            {
                Replicas = pipelineService.Replicas,
                Selector = new V1LabelSelector()
                {
                    MatchLabels = new Dictionary<string, string>()
                    {
                        { "app.kubernetes.io/name", pipelineService.Name },
                        { "app.kubernetes.io/part-of", ns }
                    }
                },
                Template = new V1PodTemplateSpec()
                {
                    Spec = new V1PodSpec()
                    {
                        Containers = new List<V1Container>()
                        {
                            new V1Container()
                            {
                                Name = pipelineService.Name,
                                ImagePullPolicy = "Always",
                                Image = null, //todo: image
                                Env = GetEnvVars(environmentVariables),
                                Resources = new V1ResourceRequirements()
                                {
                                    Limits = GetResourceLimits(pipelineService.Resources.Limits),
                                },
                                Ports = new List<V1ContainerPort>()
                                {
                                    new()
                                    {
                                        ContainerPort = pipelineService.ServicePort
                                    }
                                },
                            }
                        }
                    }
                }
            }
        };

        return deployment;
    }

    private List<V1EnvVar> GetEnvVars(IEnumerable<EnvironmentVariable> environmentVariables)
    {
        var envVars = environmentVariables.Select(e => new V1EnvVar()
        {
            Name = e.Name,
            Value = e.Value?.ToString()
        }).ToList();
        return envVars;
    }

    private Dictionary<string, ResourceQuantity> GetResourceLimits(Dictionary<ResourceUnits, string> limits)
    {
        var resourceQuantities =
            limits
                .ToDictionary(d => d.ToString().ToLower(),
                    d => new ResourceQuantity(d.Value)
                );
        return resourceQuantities;
    }
}