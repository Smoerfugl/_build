using System.CommandLine;
using Build.Commands;
using Build.Tye;
using YamlDotNet.Serialization;

namespace Build.Pipelines;

public class Commands : ICommands
{
    private readonly IValidatePipeline _validatePipeline;
    private readonly ISerializer _serializer;

    public Commands(IValidatePipeline validatePipeline, ISerializer serializer)
    {
        _validatePipeline = validatePipeline;
        _serializer = serializer;
    }

    public void Register(CommandsBuilder builder)
    {
        var command = new Command("pipeline", "Pipeline file related");

        var newPipeline = new Command("scaffold", "Generates a new pipeline.yaml");
        newPipeline.SetHandler(context =>
        {
            var pipeline = new Pipeline()
            {
                Name = "Scaffold",
                Registry = "my.registry",
                Services = new List<PipelineService>()
                {
                    new()
                    {
                        Name = "my-app",
                        Dockerfile = "dockerfile",
                        Hostname = "app",
                        Liveness = "/health",
                        Readiness = "/health",
                        Project = "Foo.Bar",
                        Replicas = 2,
                        Resources = new ServiceResources()
                        {
                            Limits = new Dictionary<ResourceUnits, string>()
                            {
                                { ResourceUnits.Memory, "500Mi" }
                            },
                            Requests = new Dictionary<ResourceUnits, string>()
                            {
                                { ResourceUnits.Memory, "500Mi" }
                            },
                        },
                        ServicePort = 3000
                    },
                },
                EnvironmentVariables = new Dictionary<string, List<EnvironmentVariable>>()
                {
                    {
                        "production", new List<EnvironmentVariable>()
                        {
                            new("Foo", "Bar")
                        }
                    },
                    {
                        "staging", new List<EnvironmentVariable>()
                        {
                            new("Foo", "Bar")
                        }
                    },
                }
            };
            var ser = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
                .Build();

            context.Console.Write(ser.Serialize(pipeline));
        });
        command.AddCommand(newPipeline);

        AddValidateCommand(command);
        builder.Add(command);
    }

    private void AddValidateCommand(Command command)
    {
        var validate = new Command("validate", "Validates pipeline.yml");

        validate.SetHandler(context =>
        {
            var pipeline = _validatePipeline.Invoke();
            var serialized = _serializer.Serialize(pipeline);
            context.Console.Out.Write(serialized);
        });

        command.AddCommand(validate);
    }
}