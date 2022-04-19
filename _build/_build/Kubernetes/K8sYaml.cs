using k8s;

namespace Build.Kubernetes;

public static class K8sYaml
{
    public static string Serialize(object obj)
    {
        return KubernetesYaml.Serialize(obj);
    }

    public static string SerializeToMultipleObjects<T>(List<T> objects)
    {
        var strings = objects.Select(d => KubernetesYaml.Serialize(d));
        return string.Join("\n---\n", strings);
    }
}