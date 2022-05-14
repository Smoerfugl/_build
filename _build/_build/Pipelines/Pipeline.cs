using Build.Environments;
using Environment = Build.Environments.Environment;

namespace Build.Pipelines;

public class Pipeline
{
    public string Registry { get; set; } = null!;
    public List<PipelineService> Services { get; set; } = new();
    public Dictionary<string, List<EnvironmentVariable>> EnvironmentVariables { get; set; } = new();
    public string Name { get; set; } = null!;

    public List<EnvironmentVariable> GetVariables(Environment env)
    {
        var environmentVariableKey =
            EnvironmentVariables.Keys.SingleOrDefault(k =>
                string.Equals(k, env.ToString(), StringComparison.CurrentCultureIgnoreCase));

        if (environmentVariableKey != null &&
            EnvironmentVariables.TryGetValue(environmentVariableKey, out var variables))
        {
            return variables.Select(d => GetVariable(env, d))
                .ToList();
        }

        return new List<EnvironmentVariable>();
    }

    private EnvironmentVariable GetVariable(Environment env, EnvironmentVariable environmentVariable)
    {
        if (environmentVariable.Value != null)
        {
            return environmentVariable;
        }

        var assumedVariableName = $"{env.ToString().ToUpper()}__{environmentVariable.Name.ToUpper()}";
        var value = System.Environment.GetEnvironmentVariable(assumedVariableName) ?? "";
        environmentVariable.Value = value;
        return environmentVariable;
    }

    public string GetNamespace(Environment env)
    {
        return $"{env.ToString().ToLower()}-{Name}";
    }
}

public class PipelineService
{
    public string Name { get; set; } = null!;
    public string? Dockerfile = null;
    public int ServicePort { get; set; }
    public int? Replicas { get; set; } = 1;
    public string? Hostname { get; set; }
    public string Project { get; set; } = null!;

    public ServiceResources Resources { get; set; } = new ServiceResources()
    {
        Limits = new Dictionary<ResourceUnits, string>()
        {
            { ResourceUnits.CPU, "250m" },
            { ResourceUnits.Memory, "64Mi" }
        },
        Requests = new Dictionary<ResourceUnits, string>()
        {
            { ResourceUnits.CPU, "500m" },
            { ResourceUnits.Memory, "256Mi" }
        }
    };
}

public class ServiceResources
{
    public Dictionary<ResourceUnits, string> Limits { get; set; } = new();
    public Dictionary<ResourceUnits, string> Requests { get; set; } = new();
}

public enum ResourceUnits
{
    CPU,
    Memory
}