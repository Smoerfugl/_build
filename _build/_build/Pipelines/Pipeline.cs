using Build.Environments;

namespace Build.Pipelines;

public class Pipeline
{
    public string Registry { get; set; } = null!;
    public List<PipelineService> Services { get; set; } = new();
    public Dictionary<string, List<EnvironmentVariable>> EnvironmentVariables { get; set; } = new();
    public string Name { get; set; } = null!;
}

public class PipelineService
{
    public string Name { get; set; } = null!;
    public int? ServicePort { get; set; }
    public int? Replicas { get; set; } = 1;
    public string? Hostname { get; set; }
    public string Project { get; set; } = null!;
}