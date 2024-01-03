using System.Collections;
using System.Text;
using Spectre.Console;

namespace Build.Kubernetes;

public interface IKubernetesConfigRepository
{
    void AddToManifesto(params object[] objects);
    Task WriteToFile(string file);
    string Get();
}
public class KubernetesConfigRepository : IKubernetesConfigRepository
{
    private readonly List<object> _objects = new();

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

    public async Task WriteToFile(string file = "kube.yml")
    {
        var yaml = K8SYaml.SerializeToMultipleObjects(_objects.AsEnumerable());
        await File.WriteAllTextAsync(file, yaml, new UTF8Encoding(false));
        AnsiConsole.WriteLine($"Saved to file {file}");
    }

    public string Get()
    {
        return K8SYaml.SerializeToMultipleObjects(_objects);
    }
}