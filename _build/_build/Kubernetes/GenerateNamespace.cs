using k8s.Models;

namespace Build.Kubernetes;

public class GenerateNamespace
{
    public GenerateNamespace(string ns)
    {
        Namespace = ns;
    }

    public V1Namespace Invoke()
    {
        return new NamespaceBuilder()
            .WithName(Namespace)
            .Build();
    }

    public string Namespace { get; set; }
}