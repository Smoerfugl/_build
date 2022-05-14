using Build.Environments;
using Build.Pipelines;

namespace Build.Tye;

public interface IGenerateTyeYaml
{
    TyeConfig Invoke();
}
public class GenerateTyeYaml : IGenerateTyeYaml
{
    private readonly Pipeline _pipelineObject;

    public GenerateTyeYaml(IEnv env, IGetPipeline getPipeline)

    {
        _pipelineObject = getPipeline.Invoke();
        Env = env;
        EnvKey = GetEnvironmentKey();
    }

    public IEnv Env { get; set; }
    public readonly string? EnvKey;

    public TyeConfig Invoke()
    {
        Console.WriteLine($"Generating Configuration for {Env.Value.ToString().ToLower()}");

        var environmentVariables = new List<EnvironmentVariable>();
        if (EnvKey != null)
        {
            environmentVariables = _pipelineObject.EnvironmentVariables[EnvKey]
                .Select(d => new EnvironmentVariable(d.Name, d.Value ?? GetVariable(d.Name.ToString().ToUpper())))
                .ToList();
        }

        var services = _pipelineObject.Services.Select(p => new TyeService($"{p.Name}-{Env.Value.ToString()}", p.Project)
        {
            Env = environmentVariables,
            Replicas = p.Replicas
        }).ToList();

        var tyeFile = new TyeConfig()
        {
            Name = _pipelineObject.Name,
            Namespace = $"{Env.ToString()?.ToLower()}-{_pipelineObject.Name}",
            Registry = _pipelineObject.Registry,
            Services = services
        };
        return tyeFile;
    }

    private object GetVariable(string variableName)
    {
        var assumedVariableName = $"{Env.ToString()?.ToUpper()}__{variableName}";
        return System.Environment.GetEnvironmentVariable(assumedVariableName) ?? "";
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