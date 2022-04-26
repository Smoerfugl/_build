using Build.Tye;

namespace Build.Kubernetes;

public interface IGenerateManifesto
{
    List<object> Invoke(TyeConfig tyeConfig);
}

public class GenerateManifesto : IGenerateManifesto
{
    public List<object> Invoke(TyeConfig tyeConfig)
    {
        var objects = new List<object>
        {
            new GenerateNamespace(tyeConfig.Namespace).Invoke()
        };
        return objects;
    }
}