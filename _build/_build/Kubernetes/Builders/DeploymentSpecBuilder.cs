using k8s.Models;

namespace Build.Kubernetes.Builders;

public class DeploymentSpecBuilder
{
    private V1DeploymentSpec _v1DeploymentSpec = new();
    private V1PodTemplateSpec _template = new();
    private IDictionary<string, string> _labels = new Dictionary<string, string>();
    private string _name = null!;
    private string _namespace = null!;
    private V1Container _container = null!;

    public DeploymentSpecBuilder()
    {
        _v1DeploymentSpec.Replicas = 1;
        _v1DeploymentSpec.Strategy = new V1DeploymentStrategy();
        _v1DeploymentSpec.Strategy.Type = "Recreate";
        _v1DeploymentSpec.Template = _template;
    }

    public DeploymentSpecBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public DeploymentSpecBuilder WithLabels(IDictionary<string, string> labels)
    {
        _labels = labels;
        return this;
    }

    public DeploymentSpecBuilder WithNamespace(string @ns)
    {
        _namespace = @ns;
        return this;
    }

    public DeploymentSpecBuilder WithContainer(V1Container container)
    {
        _container = container;
        return this;
    }

    public V1DeploymentSpec Build()
    {
        _labels.Add("app.kubernetes.io/name", _name);
        _labels.Add("app.kubernetes.io/part-of", _namespace);
        _template.Metadata = new V1ObjectMeta();
        _template.Metadata.Labels = _labels;
        _template.Spec = new V1PodSpec();
        _template.Spec.Containers = new List<V1Container>();
        _template.Spec.Containers.Add(_container);
        return _v1DeploymentSpec;
    }
}