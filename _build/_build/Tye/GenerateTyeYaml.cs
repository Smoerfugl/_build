using Build.Environments;
using Build.Pipelines;

namespace Build.Tye;

public interface IGenerateTyeYaml
{
    TyeConfig Invoke();
}
public class GenerateTyeYaml : IGenerateTyeYaml
{
    private readonly IGetPipeline _getPipeline;

    public GenerateTyeYaml(IEnv env, IGetPipeline getPipeline)

    {
        _getPipeline = getPipeline;
        Env = env;
    }

    public IEnv Env { get; set; }

    public TyeConfig Invoke()
    {
        Console.WriteLine($"Generating Configuration for {Env.Value.ToString().ToLower()}");
        var pipelineObject = _getPipeline.Invoke();
        var environmentKey = GetEnvironmentKey(pipelineObject);

        var environmentVariables = new List<EnvironmentVariable>();
        if (environmentKey != null)
        {
            environmentVariables = pipelineObject.EnvironmentVariables[environmentKey]
                .Select(d => new EnvironmentVariable(d.Name, d.Value ?? GetVariable(d.Name.ToString().ToUpper())))
                .ToList();
        }

        var services = pipelineObject.Services.Select(p => new TyeService($"{p.Name}-{Env.Value.ToString()}", p.Project)
        {
            Env = environmentVariables,
            Replicas = p.Replicas
        }).ToList();

        var tyeFile = new TyeConfig()
        {
            Name = pipelineObject.Name,
            Namespace = $"{Env.ToString()?.ToLower()}-{pipelineObject.Name}",
            Registry = pipelineObject.Registry,
            Services = services
        };
        return tyeFile;
    }

    private object GetVariable(string variableName)
    {
        var assumedVariableName = $"{Env.ToString()?.ToUpper()}__{variableName}";
        return System.Environment.GetEnvironmentVariable(assumedVariableName) ?? "";
    }

    private string? GetEnvironmentKey(Pipeline pipelineObject)
    {
        var key = pipelineObject 
            .EnvironmentVariables
            .Keys
            .SingleOrDefault(k =>
                string.Equals(k, Env.ToString(), StringComparison.CurrentCultureIgnoreCase)
            );
        return key;
    }
}