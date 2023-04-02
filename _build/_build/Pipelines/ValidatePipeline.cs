using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Build.Pipelines;

public interface IValidatePipeline
{
    Pipeline Invoke();
}

public class ValidatePipeline : IValidatePipeline
{
    private readonly IGetPipelineYaml _getPipelineYaml;
    private IDeserializer _deserializer;

    public ValidatePipeline(IGetPipelineYaml getPipelineYaml)
    {
        _getPipelineYaml = getPipelineYaml;
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public Pipeline Invoke()
    {
        var pipelineFile = _getPipelineYaml.Invoke();
        var pipelineObject = _deserializer.Deserialize<Pipeline>(pipelineFile);
        return pipelineObject;
    }
}