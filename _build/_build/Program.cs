// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.CommandLine.Parsing;
using Build.Docker;
using Build.Extensions;
using Build.Kubernetes;
using Build.Pipelines;
using Build.Environments;
using Build.MsBuild;
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

void AddTargets(Targets targets, ParseResult cmdLine)
{
    var environmentValue = cmdLine.CommandResult.GetValueForOption(environmentOption)!;
    var domainValue = cmdLine.CommandResult.GetValueForOption(domainOption)!;
    var outputPathValue = cmdLine.CommandResult.GetValueForOption(outputPath)!;
    var objects = new List<object>();
    var pipelineFile = File.ReadAllText("pipeline.yml");
    var pipelineObject = serializer.Deserialize<Pipeline>(pipelineFile);
    targets.Add("default", () => Console.WriteLine(environmentValue));

    targets.Add("GenerateTye",
        "Generate Tye.yaml file",
        () =>
        {
            var tyeFile = new GenerateTyeYaml(environmentValue.ToEnum<Env>(), pipelineObject)
                .Invoke();

            var yaml = serializer.Serialize(tyeFile);
            File.WriteAllText("tye.yaml", yaml);
        }
    );

    targets.Add("GenerateIngress", "Generate deployments",
        DependsOn("GenerateTye"),
        () =>
        {
            var ingressRoutes = new GenerateIngressRoutesRoutesList().Invoke(pipelineObject, domainValue);
            var certificates = new GenerateCertificates().Invoke(ingressRoutes);
            var @namespace = new GenerateNamespace(pipelineObject.Name).Invoke();

            objects.AddRange(ingressRoutes);
            objects.AddRange(certificates);
            objects.Add(@namespace);
        });

    targets.Add("GenerateDeployment", "Generate deployments", () =>
    {
        var deployments = new GenerateDeployments().Invoke(pipelineObject, environmentValue.ToEnum<Env>());
        var services = new GenerateServices().Invoke(deployments, environmentValue.ToEnum<Env>());
        objects.Add(deployments);
        objects.Add(services);
    });

    targets.Add("GenerateAll", "Generates all",
        DependsOn("GenerateIngress", "GenerateDeployment"),
        () => { });

    targets.Add("PushDockerImage", "docker image", () =>
    {
        var client = new DockerClientFactory().Invoke();
        // await client.Images.PushImageAsync("", new ImagePushParameters()
        // {
        //     Tag = ""
        // }, new AuthConfig(), new Progress<JSONMessage>())
    });


    var projects = pipelineObject.Services.Select(d => d.Project).ToList();
    var publishSolutions = new PublishSolutions();

    foreach (var service in projects)
    {
        targets.Add(service,
            async () => await publishSolutions.Invoke(service));
    }

    targets.Add("Publish", "Publish solutions", DependsOn(pipelineObject.Services.Select(d => d.Project).ToArray()),
        () => { });
    targets.Add("BuildDockerImages", "Builds docker images", DependsOn("Publish"), async () =>
    {
        foreach (var service in projects)
        {
            var client = new DockerClientFactory().Invoke();
            await new BuildDockerImage(client)
            .Invoke($"output/${service}");
        }
    });


    targets.Add("WriteToFile", "Write objects to file", () =>
    {
        var yaml = K8sYaml.SerializeToMultipleObjects(objects);
        File.WriteAllText(outputPathValue, yaml);
    });
}

cmd.SetHandler(async () =>
{
    // translate from System.CommandLine to Bullseye
    var cmdLine = cmd.Parse(args);

    var targetsToRun = cmdLine.CommandResult.Tokens.Select(token => token.Value).ToList();
    if (targetsToRun.Count > 0)
    {
        targetsToRun.Add("WriteToFile");
    }

    var options = new Options(Options.Definitions.Select(d => (d.Aliases[0],
        cmdLine.GetValueForOption(
            cmd.Options.OfType<Option<bool>>().Single(o => o.HasAlias(d.Aliases[0]))
        )))
    );
    var targets = new Targets();
    AddTargets(targets, cmdLine);
    options.Parallel = true;

    await targets.RunAndExitAsync(targetsToRun, options);
});
return await cmd.InvokeAsync(args);