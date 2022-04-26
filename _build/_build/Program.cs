// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.CommandLine.Parsing;
using Build.Docker;
using Build.Extensions;
using Build.Kubernetes;
using Build.Pipeline;
using Build.Tye;
using Build.Yaml;
using Bullseye;
using static Bullseye.Targets;


var cmd = new RootCommand { };
var serializer = new YamlSerializer();

var environmentOption = new Option<string>(new[] { "--environment", "-e" }, () => Env.Development.ToString(),
    "The environment to use for the build");
var domainOption = new Option<string>(new[] { "--domain" }, () => "localhost",
    "The domain to use for the build");
AddOptions(cmd);


void AddOptions(RootCommand rootCommand)
{
    rootCommand.Add(environmentOption);
    rootCommand.Add(domainOption);
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

    Target("GenerateIngress", "Generate Ingress.yaml file",
        DependsOn("GenerateTye"),
        () =>
        {
            var pipelineFile = File.ReadAllText("pipeline.yml");
            var pipelineObject = serializer.Deserialize<Pipeline>(pipelineFile);

            var ingressRoutes = new GenerateIngressRoutesRoutesList().Invoke(pipelineObject, domainValue);
            var certificates = new GenerateCertificates().Invoke(ingressRoutes);
            var @namespace = new GenerateNamespace(pipelineObject.NamespacePartial).Invoke();

            var yaml = K8sYaml.SerializeToMultipleObjects(ingressRoutes, certificates, @namespace);

            File.WriteAllText("ingress.yml", yaml);
        });

    Target("Build", "Build docker image", () =>
    {
        var client = new DockerClientFactory().Invoke();
        // await client.Images.PushImageAsync("", new ImagePushParameters()
        // {
        //     Tag = ""
        // }, new AuthConfig(), new Progress<JSONMessage>())
    });
}

cmd.SetHandler(async () =>
{
    // translate from System.CommandLine to Bullseye
    var cmdLine = cmd.Parse(args);
    var targets = cmdLine.CommandResult.Tokens.Select(token => token.Value);
    var options = new Options(Options.Definitions.Select(d => (d.Aliases[0],
        cmdLine.GetValueForOption(cmd.Options.OfType<Option<bool>>().Single(o => o.HasAlias(d.Aliases[0]))))));

    AddTargets(cmdLine);
    await RunTargetsAndExitAsync(targets, options);
});
return await cmd.InvokeAsync(args);