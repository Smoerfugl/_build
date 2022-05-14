using k8s;

namespace Build.Kubernetes;

public static class K8SYaml
{
    public static string Serialize(object obj)
    {
        return KubernetesYaml.Serialize(obj);
    }

    public static string SerializeToMultipleObjects(params object[] objects)
    {
        var list = objects.ToList();
        return SerializeToMultipleObjects(list);
    }
    public static string SerializeToMultipleObjects<T>(List<T> objects)
    {
        var strings = objects.Select(d => KubernetesYaml.Serialize(d));
        return string.Join("\n---\n", strings);
    }
}