using YamlDotNet.Serialization;

namespace Build.Pipelines;

public interface IGetPipeline
{
    Pipeline Invoke();
}

public class GetPipeline : IGetPipeline
{
    private readonly IDeserializer _deserializer;
    private readonly IGetPipelineYaml _getPipelineYaml;

    public GetPipeline(IDeserializer deserializer, IGetPipelineYaml getPipelineYaml)
    {
        _deserializer = deserializer;
        _getPipelineYaml = getPipelineYaml;
    }
    
    public Pipeline Invoke()
    {
        var pipelineFile = _getPipelineYaml.Invoke();
        var pipelineObject = _deserializer.Deserialize<Pipeline>(pipelineFile);
        return pipelineObject;
    }
}