using YamlDotNet.Serialization;

namespace Build.Pipelines;

public interface IGetPipeline
{
    Pipeline? Invoke();
}

public class GetPipeline : IGetPipeline
{
    private readonly IDeserializer _deserializer;

    public GetPipeline(IDeserializer deserializer)
    {
        _deserializer = deserializer;
    }
    
    public Pipeline? Invoke()
    {
        if (!File.Exists("pipeline.yml"))
        {
            return null;
        }

        var pipelineFile = File.ReadAllText("pipeline.yml");
        var pipelineObject = _deserializer.Deserialize<Pipeline>(pipelineFile);
        return pipelineObject;
    }
}