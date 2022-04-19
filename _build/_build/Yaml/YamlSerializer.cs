using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Build.Yaml;

public class YamlSerializer
{
    private readonly ISerializer _serializer;

    public YamlSerializer()
    {
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .DisableAliases()
            .Build();
    }
    
    public string Serialize(object obj)
    {
        var yaml = _serializer.Serialize(obj);
        return yaml;
    }
    
    public T Deserialize<T>(string yamlString)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var obj = deserializer.Deserialize<T>(yamlString);
        return obj;
    }
    
}