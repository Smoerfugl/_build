using Build.Tye;
using Environment = Build.Environments.Environment;

namespace Build.Pipelines;

public class Pipeline
{
    public string Registry { get; init; } = null!;
    public List<PipelineService> Services { get; init; } = new();
    public Dictionary<string, List<EnvironmentVariable>> EnvironmentVariables { get; init; } = new();
    public string Name { get; init; } = null!;
    public bool Https { get; set; } = false;

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
    public string Name { get; init; } = null!;
    public string? Dockerfile { get; init; } = null;
    public int ServicePort { get; init; }
    public int? Replicas { get; init; } = 1;
    public string? Hostname { get; init; }
    public string Project { get; init; } = null!;

    public string? Liveness { get; set; }
    public string? Readiness { get; set; }

    public string? StartupProbe { get; set; }

    public ServiceResources Resources { get; init; } = new ServiceResources()
    {
        Limits = new Dictionary<ResourceUnits, string>()
        {
            { ResourceUnits.CPU, "500m" },
            { ResourceUnits.Memory, "256Mi" }
        },
        Requests = new Dictionary<ResourceUnits, string>()
        {
            { ResourceUnits.CPU, "250m" },
            { ResourceUnits.Memory, "64Mi" }
        }
    };
}

public class ServiceResources
{
    public Dictionary<ResourceUnits, string> Limits { get; init; } = new();

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public Dictionary<ResourceUnits, string> Requests { get; init; } = new();
}

public enum ResourceUnits
{
    // ReSharper disable once InconsistentNaming
    CPU,
    Memory
}