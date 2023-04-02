namespace Build.Pipelines;

public interface IGetPipelineYaml
{
    string Invoke();
}

public class GetPipelineYaml : IGetPipelineYaml
{
    public string Invoke()
    {
        if (!File.Exists("pipeline.yml"))
        {
            throw new Exception("Pipeline file not found");
        }

        var pipelineFile = File.ReadAllText("pipeline.yml");
        return pipelineFile;
    }
}