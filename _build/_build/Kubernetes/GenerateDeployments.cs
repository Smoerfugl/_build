using Build.Environments;
using Build.Pipelines;
using k8s.Models;
using Environment = Build.Environments.Environment;

namespace Build.Kubernetes;

public interface IGenerateDeployments
{
    IEnumerable<V1Deployment> Invoke(string tagValue);
}

public class GenerateDeployments : IGenerateDeployments
{
    private readonly IEnv _env;
    private readonly Environment _environment;
    private readonly Pipeline? _pipeline;

    public GenerateDeployments(IGetPipeline getPipeline, IEnv env)
    {
        _pipeline = getPipeline.Invoke();
        _env = env;
        _environment = env.Value;
    }
    public IEnumerable<V1Deployment> Invoke(string tagValue)
    {
        if (_pipeline == null)
        {
            throw new Exception("Pipeline not found");
        }
        var ns = _pipeline.GetNamespace(_environment);

        var key = _pipeline.EnvironmentVariables.Keys.SingleOrDefault(d =>
            string.Equals(d, _environment.ToString(), StringComparison.CurrentCultureIgnoreCase));

        IEnumerable<EnvironmentVariable> environmentVariables = new List<EnvironmentVariable>();
        if (key != null)
        {
            environmentVariables = _pipeline.EnvironmentVariables[key];
        }

        var deployments = _pipeline.Services.Select(d =>
            GenerateDeployment(d, ns, environmentVariables, tagValue)
        );

        return deployments;
    }

    private static V1Deployment GenerateDeployment(PipelineService pipelineService, string ns,
        IEnumerable<EnvironmentVariable> environmentVariables, string tagValue)
    {
        var deployment = new V1Deployment()
        {
            Kind = "Deployment",
            ApiVersion = "apps/v1",
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
                            new()
                            {
                                Name = pipelineService.Name,
                                ImagePullPolicy = "Always",
                                Image = !string.IsNullOrWhiteSpace(tagValue) ? tagValue : "latest", //todo: image
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

    private static List<V1EnvVar> GetEnvVars(IEnumerable<EnvironmentVariable> environmentVariables)
    {
        var envVars = environmentVariables.Select(e => new V1EnvVar()
        {
            Name = e.Name,
            Value = e.Value?.ToString()
        }).ToList();
        return envVars;
    }

    private static Dictionary<string, ResourceQuantity> GetResourceLimits(Dictionary<ResourceUnits, string> limits)
    {
        var resourceQuantities =
            limits
                .ToDictionary(d => d.Key.ToString().ToLower(),
                    d => new ResourceQuantity(d.Value)
                );
        return resourceQuantities;
    }
}