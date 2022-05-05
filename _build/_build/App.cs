using System.CommandLine;
using System.CommandLine.Parsing;
using Build.Docker;
using Build.Environments;
using Build.Extensions;
using Build.Kubernetes;
using Build.MsBuild;
using Build.Pipelines;
using Bullseye;
using Microsoft.Extensions.Hosting;
using YamlDotNet.Serialization;
using static Bullseye.Targets;

namespace Build;

public class App : IHostedService
{
    private readonly IHostApplicationLifetime _applicationLifetime;

    private readonly RootCommand _rootCommand;

    private readonly ICommandArgs _commandArgs;
    // private readonly IHostApplicationLifetime _applicationLifetime;
    // private readonly RootCommand _rootCommand;
    // private readonly ICommandArgs _commandArgs;
    // private readonly ISerializer _serializer;
    // private readonly IDeserializer _deserializer;

    // public App(IHostApplicationLifetime applicationLifetime,
    //     RootCommand rootCommand,
    //     ICommandArgs commandArgs,
    //     ISerializer serializer,
    //     IDeserializer deserializer)
    // {
    //     _applicationLifetime = applicationLifetime;
    //     _rootCommand = rootCommand;
    //     _commandArgs = commandArgs;
    //     _serializer = serializer;
    //     _deserializer = deserializer;
    // }

    public App(IHostApplicationLifetime applicationLifetime, RootCommand rootCommand, ICommandArgs commandArgs)
    {
        _applicationLifetime = applicationLifetime;
        _rootCommand = rootCommand;
        _commandArgs = commandArgs;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // await D();
        var d = _rootCommand.Parse(_commandArgs.Args);
        await _rootCommand.InvokeAsync(_commandArgs.Args);
        this._applicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    // public Task D()
    // {
    //     var environmentOption = new Option<string>(new[] { "--environment", "-e" }, () => Env.Development.ToString(),
    //         "The environment to use for the build");
    //     var domainOption = new Option<string>(new[] { "--domain" }, () => "localhost",
    //         "The domain to use for the build");
    //     var outputPath = new Option<string>(new[] { "--output", "-o" }, () => "./kube.yml");
    //     var tagOption = new Option<string>(new[] { "--tag" }, () => "latest");
    //     AddOptions(_rootCommand);
    //
    //
    //     void AddOptions(Command rootCommand)
    //     {
    //         rootCommand.Add(environmentOption);
    //         rootCommand.Add(domainOption);
    //         rootCommand.Add(outputPath);
    //         rootCommand.Add(tagOption);
    //     }
    //
    //     _rootCommand.Add(new Argument("targets")
    //     {
    //         Arity = ArgumentArity.ZeroOrMore,
    //         Description =
    //             "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.",
    //     });
    //     foreach (var (aliases, description) in Options.Definitions)
    //     {
    //         _rootCommand.Add(new Option<bool>(aliases.ToArray(), description));
    //     }
    //
    //     void AddTargets(Targets targets, ParseResult cmdLine)
    //     {
    //         var environmentValue = cmdLine.CommandResult.GetValueForOption(environmentOption)!;
    //         var domainValue = cmdLine.CommandResult.GetValueForOption(domainOption)!;
    //         var outputPathValue = cmdLine.CommandResult.GetValueForOption(outputPath)!;
    //         var objects = new List<object>();
    //
    //         targets.Add("default", () => Console.WriteLine(environmentValue));
    //
    //
    //         var version = cmdLine.GetValueForOption(tagOption)!;
    //
    //         targets.Add("PushDockerImage", "docker image", () =>
    //         {
    //             var client = new DockerClientFactory().Invoke();
    //             // await client.Images.PushImageAsync("", new ImagePushParameters()
    //             // {
    //             //     Tag = ""
    //             // }, new AuthConfig(), new Progress<JSONMessage>())
    //         });
    //
    //         if (File.Exists("pipeline.yml"))
    //         {
    //             var pipelineFile = File.ReadAllText("pipeline.yml");
    //             var pipelineObject = _deserializer.Deserialize<Pipeline>(pipelineFile);
    //
    //             targets.Add("GenerateTye",
    //                 "Generate Tye.yaml file",
    //                 () =>
    //                 {
    //                     var tyeFile = new GenerateTyeYaml(environmentValue.ToEnum<Env>(), pipelineObject)
    //                         .Invoke();
    //
    //                     var yaml = _serializer.Serialize(tyeFile);
    //                     File.WriteAllText("tye.yaml", yaml);
    //                 }
    //             );
    //
    //             targets.Add("GenerateIngress", "Generate deployments",
    //                 DependsOn("GenerateTye"),
    //                 () =>
    //                 {
    //                     var ingressRoutes = new GenerateIngressRoutesRoutesList().Invoke(pipelineObject, domainValue);
    //                     var certificates = new GenerateCertificates().Invoke(ingressRoutes);
    //                     var @namespace = new GenerateNamespace(pipelineObject.Name).Invoke();
    //
    //                     objects.AddRange(ingressRoutes);
    //                     objects.AddRange(certificates);
    //                     objects.Add(@namespace);
    //                 });
    //
    //             targets.Add("GenerateDeployment", "Generate deployments", () =>
    //             {
    //                 var deployments = new GenerateDeployments().Invoke(pipelineObject, environmentValue.ToEnum<Env>());
    //                 var services = new GenerateServices().Invoke(deployments, environmentValue.ToEnum<Env>());
    //                 objects.Add(deployments);
    //                 objects.Add(services);
    //             });
    //
    //             var projects = pipelineObject.Services.Select(d => new { d.Project, d.Dockerfile }).ToList();
    //             var publishSolutions = new PublishSolutions();
    //
    //             foreach (var service in projects)
    //             {
    //                 targets.Add(service.Project,
    //                     async () => await publishSolutions.Invoke(service.Project));
    //             }
    //
    //             targets.Add("Publish", "Publish solutions",
    //                 DependsOn(pipelineObject.Services.Select(d => d.Project).ToArray()),
    //                 () => { });
    //             targets.Add("BuildDockerImages", "Builds docker images", DependsOn("Publish"), async () =>
    //             {
    //                 Console.WriteLine($"Building {string.Join(", ", projects)} with {version} tag");
    //                 foreach (var project in projects)
    //                 {
    //                     var client = new DockerClientFactory().Invoke();
    //                     await new BuildDockerImage(client)
    //                         .Invoke(pipelineObject.Registry, project.Project, project.Dockerfile, version);
    //                 }
    //             });
    //
    //             targets.Add("GenerateAll", "Generates all",
    //                 DependsOn("GenerateIngress", "GenerateDeployment"),
    //                 () => { });
    //         }
    //
    //         targets.Add("WriteToFile", "Write objects to file", () =>
    //         {
    //             var yaml = K8sYaml.SerializeToMultipleObjects(objects);
    //             File.WriteAllText(outputPathValue, yaml);
    //         });
    //     }
    //     var args = new string[0];
    //
    //     return Task.CompletedTask;
    // }
}