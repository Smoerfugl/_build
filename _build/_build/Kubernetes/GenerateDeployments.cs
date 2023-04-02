using Build.Environments;
using Build.Pipelines;
using Build.Tye;
using k8s.Models;
using Environment = Build.Environments.Environment;

namespace Build.Kubernetes;

public interface IGenerateDeployments
{
    IEnumerable<V1Deployment> Invoke(string tagValue);
}

public class GenerateDeployments : IGenerateDeployments
{
    private readonly IGetPipeline _getPipeline;
    private readonly IEnv _env;
    private readonly Environment _environment;

    public GenerateDeployments(IGetPipeline getPipeline, IEnv env)
    {
        _getPipeline = getPipeline;
        _env = env;
        _environment = env.Value;
    }

    public IEnumerable<V1Deployment> Invoke(string tagValue)
    {
        var pipeline = _getPipeline.Invoke();
        if (pipeline == null)
        {
            throw new Exception("Pipeline not found");
        }

        var ns = pipeline.GetNamespace(_environment);

        var environmentVariables = pipeline.GetVariables(_env.Value);

        var deployments = pipeline.Services.Select(pipelineService =>
            GenerateDeployment(pipeline.Registry, pipelineService, ns, environmentVariables, tagValue)
        );

        return deployments;
    }

    private V1Deployment GenerateDeployment(string pipelineRegistry, PipelineService pipelineService, string ns,
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
                    Metadata = new V1ObjectMeta()
                    {
                        Labels = new Dictionary<string, string>()
                        {
                            { "app.kubernetes.io/name", pipelineService.Name },
                            { "app.kubernetes.io/part-of", ns }
                        },
                    },
                    Spec = new V1PodSpec()
                    {
                        Containers = new List<V1Container>()
                        {
                            new()
                            {
                                Name = pipelineService.Name,
                                ImagePullPolicy = "Always",
                                Image =
                                    $"{pipelineRegistry}/{pipelineService.Project.ToLower()}:{(!string.IsNullOrWhiteSpace(tagValue) ? tagValue : "latest")}", //todo: image
                                Env = GetEnvVars(environmentVariables),
                                Resources = new V1ResourceRequirements()
                                {
                                    Limits = GetResourceLimits(pipelineService.Resources.Limits),
                                    Requests = GetResourceLimits(pipelineService.Resources.Requests),
                                },
                                Ports = new List<V1ContainerPort>()
                                {
                                    new()
                                    {
                                        ContainerPort = pipelineService.ServicePort
                                    }
                                },
                                LivenessProbe = GetLivenessProbe(pipelineService),
                                ReadinessProbe = GetReadinessProbe(pipelineService),
                                StartupProbe = GetLivenessProbe(pipelineService),
                            }
                        }
                    }
                }
            }
        };

        return deployment;
    }

    private V1Probe? GetReadinessProbe(PipelineService pipelineService)
    {
        return GetProbe(pipelineService.Readiness, pipelineService);
    }

    private V1Probe? GetLivenessProbe(PipelineService pipelineService)
    {
        return GetProbe(pipelineService.Liveness, pipelineService);
    }

    private static V1Probe? GetProbe(string? str, PipelineService pipelineService)
    {
        return string.IsNullOrWhiteSpace(str)
            ? new V1Probe()
            {
                HttpGet = new V1HTTPGetAction()
                {
                    Port = pipelineService.ServicePort,
                    Path = pipelineService.Liveness,
                },
                FailureThreshold = 5,
                InitialDelaySeconds = 3
            }
            : null;
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