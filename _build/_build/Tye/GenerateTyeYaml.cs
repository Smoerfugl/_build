using System.Runtime.InteropServices.ComTypes;

namespace Build.Tye;

public class GenerateTyeYaml
{
    private readonly Pipeline.Pipeline _pipelineObject;

    public GenerateTyeYaml(Env env, Pipeline.Pipeline pipelineObject)
    {
        _pipelineObject = pipelineObject;
        Env = env;
        _envKey = GetEnvironmentKey();
    }

    public Env Env { get; set; }
    public string? _envKey;

    public TyeConfigGenerator Invoke()
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

        var tyeFile = new TyeConfigGenerator()
        {
            Name = "Hejs",
            Namespace = $"{Env.ToString().ToLower()}-hejs",
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

public enum Env
{
    Development,
    Staging,
    Production
}