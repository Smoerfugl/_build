using k8s.Models;

namespace Build.Kubernetes.Builders;

public class DeploymentBuilder
{
    private V1Deployment _v1Deployment = new()
    {
        Kind = "Deployment",
        ApiVersion = "apps/v1",
        Metadata = new V1ObjectMeta(),
        Spec = new V1DeploymentSpec()
        {
            Template = new V1PodTemplateSpec()
            {
                Metadata = new V1ObjectMeta()
                {
                    
                }
            }
        },
    };
    private IDictionary<string, string> _labels = new Dictionary<string, string>();
    private string _name = null!;
    private string _namespace = null!;
    private V1DeploymentSpec _spec = null!;

    public DeploymentBuilder()
    {
    }

    public DeploymentBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public DeploymentBuilder WithLabels(IDictionary<string, string> labels)
    {
        _labels = labels;
        return this;
    }

    public DeploymentBuilder WithNamespace(string @ns)
    {
        _namespace = @ns;
        return this;
    }

    public DeploymentBuilder WithSpec(V1DeploymentSpec v1DeploymentSpec)
    {
        _spec = v1DeploymentSpec;
        return this;
    }
    
    public V1Deployment Build()
    {
        _labels.Add("app.kubernetes.io/name", _name);
        _labels.Add("app.kubernetes.io/part-of", _namespace);
        _v1Deployment.Metadata.Name = _name;
        _v1Deployment.Metadata.NamespaceProperty = _namespace;
        _v1Deployment.Spec = _spec;
        return _v1Deployment;
    }
}