using System.Runtime.InteropServices.ComTypes;

namespace Build.Environments;

public interface IGenerateTyeYaml
{
    TyeConfig Invoke();
}
public class GenerateTyeYaml : IGenerateTyeYaml
{
    private readonly Pipelines.Pipeline _pipelineObject;

    public GenerateTyeYaml(Env env, Pipelines.Pipeline pipelineObject)
    {
        _pipelineObject = pipelineObject;
        Env = env;
        _envKey = GetEnvironmentKey();
    }

    public Env Env { get; set; }
    public string? _envKey;

    public TyeConfig Invoke()
    {
        Console.WriteLine($"Generating Configuration for {Env.ToString().ToLower()}");

        var environmentVariables = new List<EnvironmentVariable>();
        if (_envKey != null)
        {
            environmentVariables = _pipelineObject.EnvironmentVariables[_envKey]
                .Select(d => new EnvironmentVariable(d.Name, d.Value ?? GetVariable(d.Name.ToString().ToUpper())))
                .ToList();
        }

        var services = _pipelineObject.Services.Select(p => new TyeService($"{p.Name}-{Env.ToString()}", p.Project)
        {
            Env = environmentVariables,
            Replicas = p.Replicas
        }).ToList();

        var tyeFile = new TyeConfig()
        {
            Name = _pipelineObject.Name,
            Namespace = $"{Env.ToString().ToLower()}-{_pipelineObject.Name}",
            Registry = _pipelineObject.Registry,
            Services = services
        };
        return tyeFile;
    }

    private object? GetVariable(string variableName)
    {
        var assumedVariableName = $"{Env.ToString().ToUpper()}__{variableName}";
        return Environment.GetEnvironmentVariable(assumedVariableName) ?? "";
    }

    private string? GetEnvironmentKey()
    {
        var key = _pipelineObject
            .EnvironmentVariables
            .Keys
            .SingleOrDefault(k =>
                string.Equals(k, Env.ToString(), StringComparison.CurrentCultureIgnoreCase)
            );
        return key;
    }
}