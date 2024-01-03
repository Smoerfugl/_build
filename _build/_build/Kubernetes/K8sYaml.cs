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
    public static string SerializeToMultipleObjects(IEnumerable<object> objects)
    {
        var yaml = KubernetesYaml.SerializeAll(objects);
        return yaml;
    }
}