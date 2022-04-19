using k8s.Models;
using Microsoft.IdentityModel.Tokens;

namespace Build.Kubernetes;

public class CertManagerCertificateBuilder
{
    private string _name = null!;
    private string _secretName = null!;
    private List<string> _dnsNames = null!;
    private string _namespace = null!;
    private IDictionary<string,string> _labels = new Dictionary<string, string>();

    public CertManagerCertificateBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CertManagerCertificateBuilder WithSecretName(string secretName)
    {
        _secretName = secretName;
        return this;
    }

    public CertManagerCertificateBuilder WithNamespace(string ns)
    {
        _namespace = ns;
        return this;
    }

    public CertManagerCertificateBuilder WithDnsNames(params string[] dnsNames)
    {
        _dnsNames = dnsNames.ToList();
        return this;
    }

    public CertManagerCertificate Build()
    {
        if (_dnsNames.IsNullOrEmpty())
        {
            throw new Exception("DNS names must be specified");
        }
        
        _labels.Add("app.kubernetes.io/name", _name);
        _labels.Add("app.kubernetes.io/part-of", _namespace);
        
        return new CertManagerCertificate()
        {
            Metadata = new V1ObjectMeta()
            {
                Name = _name,
                NamespaceProperty = _namespace,
                Labels = _labels
            },
            Spec = new V1CertificateSpec()
            {
                DnsNames = _dnsNames,
                SecretName = _secretName,
                IssuerRef = new IssuerRef(),
                CommonName = _dnsNames.First()
            }
            
        };
    }
}

public class CertManagerCertificate
{
    public string ApiVersion { get; set; } = "cert-manager.io/v1";
    public string Kind { get; set; } = "Certificate";
    public V1ObjectMeta Metadata { get; set; } = null!;
    public V1CertificateSpec Spec { get; set; } = null!;
}

public class V1CertificateSpec
{
    public string CommonName {get; set; } = null!;
    public string SecretName { get; set; } = null!;
    public List<string> DnsNames { get; set; } = null!;
    public IssuerRef IssuerRef { get; set; } = new();
}

public class IssuerRef
{
    public string Name { get; set; } = "letsencrypt-prod";
    public string Kind { get; set; } = "ClusterIssuer";
}