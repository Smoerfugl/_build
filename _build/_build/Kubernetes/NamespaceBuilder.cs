using System.Runtime.CompilerServices;
using k8s.Models;

namespace Build.Kubernetes;

public class NamespaceBuilder
{
    private string _apiVersion;
    private string _kind;
    private string _name;

    public NamespaceBuilder()
    {
        _apiVersion = "v1";
        _kind = "Namespace";
        _name = "default";
    }
    public NamespaceBuilder(string apiVersion = "v1", 
        string kind = "Namespace", 
        string name = "default")
    {
        _apiVersion = apiVersion;
        _kind = kind;
        _name = name;
    }
    
    public NamespaceBuilder WithApiVersion(string apiVersion)
    {
        _apiVersion = apiVersion;
        return this;
    }
    
    public NamespaceBuilder WithKind(string kind)
    {
        _kind = kind;
        return this;
    }
    
    public NamespaceBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public V1Namespace Build()
    {
        return new V1Namespace()
        {
            ApiVersion = _apiVersion,
            Kind = _kind,
            Metadata = new V1ObjectMeta()
            {
                Name = _name 
            }
        };
    }
}