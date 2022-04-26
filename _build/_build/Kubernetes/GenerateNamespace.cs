using Build.Kubernetes.Builders;
using k8s.Models;

namespace Build.Kubernetes;

public interface IGenerateNamespace
{
    V1Namespace Invoke();
}

public class GenerateNamespace : IGenerateNamespace
{
    private readonly string _ns;

    public GenerateNamespace(string ns)
    {
        _ns = ns.ToLower();
    }

    public V1Namespace Invoke()
    {
        return new NamespaceBuilder()
            .WithName(_ns)
            .Build();
    }

}