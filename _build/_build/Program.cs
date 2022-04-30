// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.CommandLine.Parsing;
using Build.Docker;
using Build.Extensions;
using Build.Kubernetes;
using Build.Pipelines;
using Build.Environments;
using Build.Yaml;
using Bullseye;
using static Bullseye.Targets;


var cmd = new RootCommand { };
var serializer = new YamlSerializer();

var environmentOption = new Option<string>(new[] { "--environment", "-e" }, () => Env.Development.ToString(),
    "The environment to use for the build");
var domainOption = new Option<string>(new[] { "--domain" }, () => "localhost",
    "The domain to use for the build");
var outputPath = new Option<string>(new[] { "--output", "-o" }, () => "./kube.yml");
AddOptions(cmd);


void AddOptions(Command rootCommand)
{
    rootCommand.Add(environmentOption);
    rootCommand.Add(domainOption);
    rootCommand.Add(outputPath);
}

cmd.Add(new Argument("targets")
{
    Arity = ArgumentArity.ZeroOrMore,
    Description =
        "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.",
});
foreach (var (aliases, description) in Options.Definitions)
{
    cmd.Add(new Option<bool>(aliases.ToArray(), description));
}

void AddTargets(ParseResult cmdLine)
{
    var environmentValue = cmdLine.CommandResult.GetValueForOption(environmentOption)!;
    var domainValue = cmdLine.CommandResult.GetValueForOption(domainOption)!;
    var outputPathValue = cmdLine.CommandResult.GetValueForOption(outputPath)!;
    var objects = new List<object>();
    Target("default",
        () => Console.WriteLine(environmentValue));

    Target("GenerateTye",
        "Generate Tye.yaml file",
        () =>
        {
            var pipelineFile = File.ReadAllText("pipeline.yml");
            var pipelineObject = serializer.Deserialize<Pipeline>(pipelineFile);
            var tyeFile = new GenerateTyeYaml(environmentValue.ToEnum<Env>(), pipelineObject)
                .Invoke();

            var yaml = serializer.Serialize(tyeFile);
            File.WriteAllText("tye.yaml", yaml);
        }
    );

    Target("GenerateIngress", "Generate deployments",
        DependsOn("GenerateTye"),
        () =>
        {
            var pipelineFile = File.ReadAllText("pipeline.yml");
            var pipelineObject = serializer.Deserialize<Pipeline>(pipelineFile);

            var ingressRoutes = new GenerateIngressRoutesRoutesList().Invoke(pipelineObject, domainValue);
            var certificates = new GenerateCertificates().Invoke(ingressRoutes);
            var @namespace = new GenerateNamespace(pipelineObject.Name).Invoke();

            objects.AddRange(ingressRoutes);
            objects.AddRange(certificates);
            objects.Add(@namespace);
        });

    Target("GenerateDeployment", "Generate deployments", () =>
    {
        var pipelineFile = File.ReadAllText("pipeline.yml");
        var pipelineObject = serializer.Deserialize<Pipeline>(pipelineFile);

        var deployments = new GenerateDeployments().Invoke(pipelineObject, environmentValue.ToEnum<Env>());
        var services = new GenerateServices().Invoke(deployments, environmentValue.ToEnum<Env>());
        objects.Add(deployments);
        objects.Add(services);
    });

    Target("GenerateAll", "Generates all",
        DependsOn("GenerateIngress", "GenerateDeployment"),
        () => { });

    Target("Build", "Build docker image", () =>
    {
        var client = new DockerClientFactory().Invoke();
        // await client.Images.PushImageAsync("", new ImagePushParameters()
        // {
        //     Tag = ""
        // }, new AuthConfig(), new Progress<JSONMessage>())
    });

    Target("WriteToFile", "Write objects to file", () =>
    {
        var yaml = K8sYaml.SerializeToMultipleObjects(objects);
        File.WriteAllText(outputPathValue, yaml);
    });
}

cmd.SetHandler(async () =>
{
    // translate from System.CommandLine to Bullseye
    var cmdLine = cmd.Parse(args);

    var targets = cmdLine.CommandResult.Tokens.Select(token => token.Value).ToList();
    if (targets.Count == 0)
    {
        targets.Add("WriteToFile");
    }

    var options = new Options(Options.Definitions.Select(d => (d.Aliases[0],
        cmdLine.GetValueForOption(
            cmd.Options.OfType<Option<bool>>().Single(o => o.HasAlias(d.Aliases[0]))
        )))
    );

    AddTargets(cmdLine);
    await RunTargetsAndExitAsync(targets, options);
});
return await cmd.InvokeAsync(args);