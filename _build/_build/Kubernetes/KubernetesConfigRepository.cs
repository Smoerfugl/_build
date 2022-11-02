using System.Collections;

namespace Build.Kubernetes;

public interface IKubernetesConfigRepository
{
    string FileName {get; set; }
    void AddToManifesto(params object[] objects);
    Task WriteToFile();
}
public class KubernetesConfigRepository : IKubernetesConfigRepository
{
    public string FileName { get; set; } = "kube.yml";
    private List<object> _objects = new();

    public void AddToManifesto(params object[] objects)
    {
        var any = objects.Any(type => type.GetType().IsAssignableTo(typeof(IEnumerable)));
        if (!any)
        {
            _objects.AddRange(objects);
            return;
        }

        var collection = new List<object>();
        foreach (var obj in objects)
        {
            if (obj.GetType().IsAssignableTo(typeof(IEnumerable)))
            {
                collection.AddRange((IEnumerable<object>) obj);
            }
            else
            {
                collection.Add(obj);
            }
        }
        
        _objects.AddRange(collection);
    }

    public async Task WriteToFile()
    {
        var yaml = K8SYaml.SerializeToMultipleObjects(_objects);
        await File.WriteAllTextAsync(FileName, yaml);
    }
}