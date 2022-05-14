using YamlDotNet.Serialization;

namespace Build.Pipelines;

public interface IGetPipeline
{
    Pipeline Invoke();
}

public class GetPipeline : IGetPipeline
{
    private readonly IDeserializer _deserializer;

    public GetPipeline(IDeserializer deserializer)
    {
        _deserializer = deserializer;
    }
    
    public Pipeline Invoke()
    {
        if (!File.Exists("pipeline.yml"))
        {
            throw new Exception("Pipeline file not found");
        }

        var pipelineFile = File.ReadAllText("pipeline.yml");
        var pipelineObject = _deserializer.Deserialize<Pipeline>(pipelineFile);
        return pipelineObject;
    }
}