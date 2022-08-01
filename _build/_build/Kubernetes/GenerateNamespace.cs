using Build.Kubernetes.Builders;
using Build.Environments;
using Build.Pipelines;
using k8s.Models;

namespace Build.Kubernetes;

public interface IGenerateNamespace
{
    V1Namespace Invoke();
}

public class GenerateNamespace : IGenerateNamespace
{
    private readonly IGetPipeline _getPipeline;
    private readonly IEnv _env;

    public GenerateNamespace(IGetPipeline getPipeline, IEnv env)
    {
        _getPipeline = getPipeline;
        _env = env;
    }

    public V1Namespace Invoke()
    {
        var pipeline = _getPipeline.Invoke();
        var ns = $"{_env.Value.ToString().ToLower()}-{pipeline.Name}";
        return new NamespaceBuilder()
            .WithName(ns)
            .Build();
    }

}